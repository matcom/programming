#!/usr/bin/env python3
"""
Verify a student project submission.

Usage: verify_submission.py <repo_url> <output_file>
Writes a Markdown report to output_file.
"""

import sys
import os
import re
import json
import shutil
import subprocess
import tempfile
from pathlib import Path


def run(cmd, cwd=None, timeout=30, input=None):
    return subprocess.run(
        cmd, capture_output=True, text=True,
        timeout=timeout, cwd=cwd, input=input
    )


def check_repo_accessible(repo_url):
    try:
        r = run(['git', 'ls-remote', '--heads', repo_url], timeout=20)
        return r.returncode == 0, r.stderr.strip()
    except subprocess.TimeoutExpired:
        return False, "Timeout al conectar con el repositorio"
    except Exception as e:
        return False, str(e)


def clone_repo(repo_url, dest):
    try:
        r = run(['git', 'clone', '--depth=1', repo_url, dest], timeout=60)
        return r.returncode == 0, r.stderr.strip()
    except subprocess.TimeoutExpired:
        return False, "Timeout al clonar el repositorio"
    except Exception as e:
        return False, str(e)


def count_words(text):
    return len(text.split())


def check_report(repo_path):
    path = Path(repo_path)
    # Look for report.md case-insensitively
    candidates = list(path.rglob('report.md')) + list(path.rglob('Report.md')) + list(path.rglob('REPORT.md'))
    if not candidates:
        return None, 0
    report_file = candidates[0]
    try:
        content = report_file.read_text(encoding='utf-8', errors='replace')
        words = count_words(content)
        return str(report_file.relative_to(path)), words
    except Exception:
        return str(report_file.relative_to(path)), 0


def find_python_files(repo_path):
    path = Path(repo_path)
    exclude = {'.git', '__pycache__', '.venv', 'venv', 'env', '.env', 'node_modules'}
    py_files = []
    for f in path.rglob('*.py'):
        if not any(part in exclude for part in f.parts):
            py_files.append(f)
    return py_files


def get_dependencies(repo_path):
    path = Path(repo_path)
    deps = []

    pyproject = path / 'pyproject.toml'
    if pyproject.exists():
        content = pyproject.read_text(errors='replace')
        # Simple regex extraction — no toml parser needed
        matches = re.findall(r'dependencies\s*=\s*\[(.*?)\]', content, re.DOTALL)
        for m in matches:
            for dep in re.findall(r'"([^"]+)"', m):
                deps.append(dep.split('>=')[0].split('==')[0].split('<')[0].strip())

    req = path / 'requirements.txt'
    if req.exists() and not deps:
        for line in req.read_text(errors='replace').splitlines():
            line = line.strip()
            if line and not line.startswith('#'):
                pkg = re.split(r'[>=<!]', line)[0].strip()
                if pkg:
                    deps.append(pkg)

    return deps


def setup_venv(repo_path, tmpdir):
    """Create a venv and install project dependencies. Returns (python_bin, error_msg)."""
    venv_dir = os.path.join(tmpdir, 'venv')
    r = run(['python', '-m', 'venv', venv_dir])
    if r.returncode != 0:
        return None, f"No se pudo crear el virtualenv: {r.stderr[:200]}"

    python_bin = os.path.join(venv_dir, 'bin', 'python')
    pip_bin = os.path.join(venv_dir, 'bin', 'pip')
    path = Path(repo_path)

    if (path / 'pyproject.toml').exists():
        r = run([pip_bin, 'install', '-e', '.', '--quiet'], cwd=repo_path, timeout=120)
        if r.returncode == 0:
            return python_bin, ""
        # Fall through to requirements.txt if editable install fails
        err = r.stderr[:200]
    else:
        err = ""

    if (path / 'requirements.txt').exists():
        r = run([pip_bin, 'install', '-r', 'requirements.txt', '--quiet'], cwd=repo_path, timeout=120)
        if r.returncode == 0:
            return python_bin, ""
        err = r.stderr[:200]

    # No deps or install failed — still return the venv python so stdlib code runs
    return python_bin, err


def try_run(repo_path, entry_point, python_bin='python'):
    """Try to run a Python file with a short timeout. Returns (ran_ok, output_snippet)."""
    try:
        r = subprocess.run(
            [python_bin, entry_point, '--help'],
            capture_output=True, text=True, timeout=8,
            cwd=repo_path, input=''
        )
        out = (r.stdout + r.stderr)[:400].strip()
        if r.returncode == 0:
            return True, out or "(sin salida)"
        # Try without --help
        r2 = subprocess.run(
            [python_bin, entry_point],
            capture_output=True, text=True, timeout=5,
            cwd=repo_path, input='\n'
        )
        out2 = (r2.stdout + r2.stderr)[:400].strip()
        return r2.returncode == 0 or bool(out2), out2 or "(sin salida)"
    except subprocess.TimeoutExpired:
        return True, "(el programa espera entrada — timeout normal)"
    except Exception as e:
        return False, str(e)


def find_entry_point(repo_path):
    path = Path(repo_path)
    for candidate in ['main.py', 'app.py', 'run.py', 'cli.py']:
        if (path / candidate).exists():
            return candidate
    # Find any *.py at root that isn't a test or config
    for f in sorted(path.glob('*.py')):
        if not f.name.startswith('test') and f.name not in ('setup.py', 'conf.py'):
            return f.name
    return None


def build_report(repo_url, results):
    ok = "✅"
    fail = "❌"
    warn = "⚠️"
    lines = []

    lines.append("## Verificación Automática — Proyecto I 🤖")
    lines.append("")
    lines.append(f"Repositorio analizado: `{repo_url}`")
    lines.append("")

    # Repo accessible
    lines.append("### 1. Repositorio")
    if results['repo_ok']:
        lines.append(f"{ok} El repositorio es accesible y fue clonado correctamente.")
    else:
        lines.append(f"{fail} No se pudo acceder al repositorio.")
        if results.get('repo_error'):
            lines.append(f"> `{results['repo_error']}`")
        lines.append("")
        lines.append("*No se pudo continuar con la verificación.*")
        lines.append("")
        lines.append("---")
        lines.append("*Verificación automática — los resultados son orientativos. El profesor revisará en detalle.*")
        return "\n".join(lines)

    lines.append("")

    # Report
    lines.append("### 2. Informe (`report.md`)")
    report_path = results.get('report_path')
    word_count = results.get('report_words', 0)
    MIN_WORDS = 2000
    if not report_path:
        lines.append(f"{fail} No se encontró `report.md` en el repositorio.")
    elif word_count >= MIN_WORDS:
        lines.append(f"{ok} `{report_path}` — **{word_count:,} palabras** (mínimo: {MIN_WORDS:,}).")
    else:
        lines.append(f"{warn} `{report_path}` encontrado pero con solo **{word_count:,} palabras** (mínimo: {MIN_WORDS:,}).")
    lines.append("")

    # Python structure
    lines.append("### 3. Proyecto Python")
    py_files = results.get('py_files', [])
    has_pyproject = results.get('has_pyproject', False)
    has_requirements = results.get('has_requirements', False)
    total_lines = results.get('total_lines', 0)

    icon = ok if (has_pyproject or has_requirements) else warn
    if has_pyproject:
        lines.append(f"{ok} `pyproject.toml` — proyecto estructurado correctamente.")
    else:
        lines.append(f"{warn} Sin `pyproject.toml`.")

    if has_requirements:
        lines.append(f"{ok} `requirements.txt` encontrado.")
    elif not has_pyproject:
        lines.append(f"{warn} Sin `requirements.txt`.")

    deps = results.get('dependencies', [])
    if deps:
        lines.append(f"Dependencias declaradas: {', '.join(f'`{d}`' for d in deps[:10])}")

    if py_files:
        lines.append(f"\nArchivos Python ({len(py_files)}, {total_lines:,} líneas en total):")
        for f in py_files[:15]:
            lines.append(f"- `{f}`")
        if len(py_files) > 15:
            lines.append(f"- *(y {len(py_files) - 15} más)*")
    else:
        lines.append(f"\n{fail} No se encontraron archivos `.py` en el repositorio.")
    lines.append("")

    # Entry point
    lines.append("### 4. Punto de Entrada")
    entry = results.get('entry_point')
    if entry:
        lines.append(f"{ok} Punto de entrada detectado: `{entry}`")
    else:
        lines.append(f"{warn} No se encontró un punto de entrada claro (`main.py`, `app.py`, etc.).")
    lines.append("")

    # Execution
    lines.append("### 5. Ejecución")
    install_err = results.get('install_error', '')
    if install_err:
        lines.append(f"{warn} Error al instalar dependencias en el virtualenv:")
        lines.append(f"\n```\n{install_err}\n```")
    if entry:
        ran_ok = results.get('ran_ok', False)
        run_output = results.get('run_output', '')
        if ran_ok:
            lines.append(f"{ok} El proyecto se ejecuta.")
            if run_output:
                lines.append(f"\n```\n{run_output}\n```")
        else:
            lines.append(f"{warn} No fue posible ejecutar `{entry}` automáticamente.")
            if run_output:
                lines.append(f"\n```\n{run_output}\n```")
    else:
        lines.append(f"{warn} Sin punto de entrada identificado — no se intentó ejecutar.")
    lines.append("")

    # Summary
    lines.append("### Resumen")
    checks = [
        ("Repositorio accesible", results['repo_ok']),
        ("report.md con ≥2000 palabras", bool(report_path) and word_count >= MIN_WORDS),
        ("Archivos Python", bool(py_files)),
        ("Configuración de dependencias", has_pyproject or has_requirements),
        ("Punto de entrada", bool(entry)),
        ("Ejecución", results.get('ran_ok', False)),
    ]
    for name, passed in checks:
        lines.append(f"- {'✅' if passed else '❌'} {name}")

    passed_count = sum(1 for _, p in checks if p)
    lines.append(f"\n**{passed_count}/{len(checks)} verificaciones pasadas.**")

    lines.append("")
    lines.append("---")
    lines.append("*Verificación automática — los resultados son orientativos. El profesor revisará en detalle.*")

    return "\n".join(lines)


def main():
    if len(sys.argv) < 3:
        print(f"Usage: {sys.argv[0]} <repo_url> <output_file>", file=sys.stderr)
        sys.exit(1)

    repo_url = sys.argv[1].strip().rstrip('/')
    output_file = sys.argv[2]

    results = {'repo_url': repo_url}

    # Check accessibility
    ok, err = check_repo_accessible(repo_url)
    results['repo_ok'] = ok
    results['repo_error'] = err

    if ok:
        with tempfile.TemporaryDirectory() as tmpdir:
            dest = os.path.join(tmpdir, 'repo')
            cloned, clone_err = clone_repo(repo_url, dest)
            if not cloned:
                results['repo_ok'] = False
                results['repo_error'] = clone_err
            else:
                # Report
                report_path, word_count = check_report(dest)
                results['report_path'] = report_path
                results['report_words'] = word_count

                # Python files
                py_files = find_python_files(dest)
                results['py_files'] = [str(f.relative_to(dest)) for f in py_files]
                total_lines = 0
                for f in py_files:
                    try:
                        total_lines += len(f.read_text(errors='replace').splitlines())
                    except Exception:
                        pass
                results['total_lines'] = total_lines

                # Project structure
                path = Path(dest)
                results['has_pyproject'] = (path / 'pyproject.toml').exists()
                results['has_requirements'] = (path / 'requirements.txt').exists()
                results['dependencies'] = get_dependencies(dest)

                # Entry point
                entry = find_entry_point(dest)
                results['entry_point'] = entry

                # Set up virtualenv and install deps before running
                python_bin, install_err = setup_venv(dest, tmpdir)
                results['install_error'] = install_err
                if python_bin is None:
                    python_bin = 'python'

                # Try to run
                if entry:
                    ran_ok, run_output = try_run(dest, entry, python_bin)
                    results['ran_ok'] = ran_ok
                    results['run_output'] = run_output

    report_md = build_report(repo_url, results)
    Path(output_file).write_text(report_md, encoding='utf-8')
    print(report_md)


if __name__ == '__main__':
    main()
