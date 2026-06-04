# Guía de Entrega del Proyecto I — 2025-2026

Esta guía te explica paso a paso cómo entregar tu primer proyecto de programación.
Sigue cada sección en orden. Si tienes algún problema, pregunta en clases o abre un Issue con la etiqueta `ayuda`.

---

## Índice

1. [Crear cuenta en GitHub](#1-crear-cuenta-en-github)
2. [Crear tu repositorio](#2-crear-tu-repositorio)
3. [Instalar Git y configurar VSCode](#3-instalar-git-y-configurar-vscode)
4. [Conectar tu computadora con GitHub](#4-conectar-tu-computadora-con-github)
5. [Clonar el repositorio en tu computadora](#5-clonar-el-repositorio-en-tu-computadora)
6. [Estructurar tu proyecto](#6-estructurar-tu-proyecto)
7. [Tu primer commit y push](#7-tu-primer-commit-y-push)
8. [Verificar en GitHub](#8-verificar-en-github)
9. [Crear el Issue de entrega](#9-crear-el-issue-de-entrega)

---

## 1. Crear cuenta en GitHub

**GitHub** es una plataforma donde los programadores guardan y comparten su código. Vas a necesitar una cuenta gratuita.

1. Ve a **https://github.com**
2. Haz clic en **Sign up** (esquina superior derecha)
3. Introduce tu correo electrónico, elige una contraseña y un nombre de usuario
   - El nombre de usuario será visible públicamente. Usa algo profesional (ej: `juan-perez`, `jperez-matcom`)
4. Verifica tu correo electrónico cuando GitHub te envíe el mensaje de confirmación
5. En las opciones de plan, elige **Free** (gratis)

> Si ya tienes cuenta, salta al paso 2.

---

## 2. Crear tu repositorio

Un **repositorio** (o "repo") es la carpeta donde vivirá todo tu proyecto en GitHub.

1. Inicia sesión en **https://github.com**
2. Haz clic en el botón verde **New** (o en el `+` de la esquina superior derecha → **New repository**)
3. Rellena el formulario:
   - **Repository name**: un nombre descriptivo, ej: `proyecto-programacion-2026`
   - **Description**: una línea describiendo tu proyecto (opcional pero recomendado)
   - **Visibility**: elige **Public** (necesario para que el verificador automático pueda leerlo)
   - Marca la casilla **Add a README file**
4. Haz clic en **Create repository**

GitHub te llevará a la página de tu nuevo repositorio. La URL será algo como:
`https://github.com/tu-usuario/proyecto-programacion-2026`

**Guarda esta URL** — la necesitarás al final para el Issue de entrega.

---

## 3. Instalar Git y configurar VSCode

### Instalar Git

Abre una terminal (Ctrl+Alt+T en la mayoría de distribuciones Linux) y ejecuta:

```bash
sudo apt update
sudo apt install git
```

Verifica que se instaló correctamente:

```bash
git --version
# Debería mostrar algo como: git version 2.43.0
```

### Instalar VSCode (si no lo tienes)

```bash
sudo apt install wget gpg
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > packages.microsoft.gpg
sudo install -D -o root -g root -m 644 packages.microsoft.gpg /etc/apt/keyrings/packages.microsoft.gpg
echo "deb [arch=amd64 signed-by=/etc/apt/keyrings/packages.microsoft.gpg] https://packages.microsoft.com/repos/code stable main" | sudo tee /etc/apt/sources.list.d/vscode.list > /dev/null
sudo apt update
sudo apt install code
```

### Configurar tu identidad en Git

Git necesita saber quién eres para firmar tus commits. Ejecuta estos comandos con tu nombre real y el correo de tu cuenta de GitHub:

```bash
git config --global user.name "Juan Pérez"
git config --global user.email "tu-correo@ejemplo.com"
```

---

## 4. Conectar tu computadora con GitHub

GitHub ya no permite usar contraseña directamente desde la terminal. La forma más sencilla es usar la **GitHub CLI** (`gh`).

### Instalar GitHub CLI

```bash
sudo apt install gh
```

### Autenticarte

```bash
gh auth login
```

El programa te hará varias preguntas. Responde así:

- **Where do you use GitHub?** → `GitHub.com`
- **What is your preferred protocol?** → `HTTPS`
- **Authenticate Git with your GitHub credentials?** → `Yes`
- **How would you like to authenticate?** → `Login with a web browser`

Copia el código de 8 caracteres que aparece en pantalla, presiona Enter, y en el navegador que se abre, pega ese código y autoriza la aplicación.

Cuando termine verás: `✓ Logged in as tu-usuario`.

---

## 5. Clonar el repositorio en tu computadora

**Clonar** significa descargar una copia del repositorio de GitHub a tu computadora para poder trabajar en él.

### Opción A: desde la terminal

```bash
# Sustituye la URL por la de tu repositorio
git clone https://github.com/tu-usuario/proyecto-programacion-2026.git
cd proyecto-programacion-2026
code .
```

El último comando abre el proyecto en VSCode.

### Opción B: desde VSCode

1. Abre VSCode
2. Presiona **Ctrl+Shift+P** para abrir la paleta de comandos
3. Escribe `Git: Clone` y presiona Enter
4. Pega la URL de tu repositorio y presiona Enter
5. Elige la carpeta donde quieres guardar el proyecto
6. Haz clic en **Open** cuando VSCode te pregunta si quieres abrir el repositorio

---

## 6. Estructurar tu proyecto

Tu repositorio debe tener esta estructura mínima:

```
proyecto-programacion-2026/
├── main.py              ← punto de entrada de tu programa
├── report.md            ← informe del proyecto (mínimo 2000 palabras)
└── requirements.txt     ← dependencias de Python (si usas librerías externas)
```

### El informe (`report.md`)

El informe debe tener **al menos 2000 palabras** y describir:

- Qué hace tu programa
- Cómo lo diseñaste y por qué tomaste las decisiones que tomaste
- Qué aprendiste durante el desarrollo
- Cómo se usa el programa (con ejemplos)
- Dificultades que encontraste y cómo las resolviste

Escríbelo en Markdown. Si no conoces Markdown, es muy sencillo:

```markdown
# Título principal

## Sección

Texto normal. **Negrita**, *cursiva*.

- Elemento de lista
- Otro elemento

```python
# Código Python
print("Hola")
```
```

### El programa (`main.py`)

Tu programa debe poder ejecutarse desde la terminal:

```bash
python main.py
```

No importa si es un juego de texto, una calculadora, un gestor de tareas, o cualquier otra aplicación de consola. Lo importante es que **haga algo** y que se pueda ejecutar.

### Dependencias (`requirements.txt`)

Si tu programa usa librerías externas (ej: `requests`, `colorama`), créa este archivo con una librería por línea:

```
requests==2.32.0
colorama==0.4.6
```

Si solo usas la biblioteca estándar de Python, no necesitas este archivo.

---

## 7. Tu primer commit y push

### ¿Qué es un commit?

Un **commit** es como una fotografía del estado de tu proyecto en un momento dado. Git guarda el historial de todos tus commits, lo que te permite ver cómo evolucionó tu código y volver atrás si algo sale mal.

### Hacer commits desde VSCode

1. En VSCode, haz clic en el ícono de **Source Control** en la barra lateral izquierda (parece un árbol con ramas, o presiona **Ctrl+Shift+G**)
2. Verás los archivos que has modificado bajo **Changes**
3. Pasa el cursor sobre un archivo y haz clic en el **`+`** para añadirlo al commit (esto se llama "staging")
4. En el campo de texto que dice **Message**, escribe un mensaje descriptivo: `Agrega main.py con la lógica principal`
5. Haz clic en el botón **Commit** (✓)

### Hacer commits desde la terminal

```bash
# Ver qué archivos han cambiado
git status

# Añadir archivos al commit
git add main.py report.md

# O añadir todos los archivos a la vez
git add .

# Crear el commit con un mensaje
git commit -m "Agrega implementación inicial y report"
```

### Hacer push (subir a GitHub)

**Push** envía tus commits locales a GitHub.

**Desde VSCode:** En el panel de Source Control, haz clic en los `...` y selecciona **Push**, o usa el botón de sincronización (las flechas circulares) en la barra inferior.

**Desde la terminal:**

```bash
git push
```

La primera vez puede pedirte que configures la rama:

```bash
git push --set-upstream origin main
```

---

## 8. Verificar en GitHub

1. Ve a la página de tu repositorio en GitHub: `https://github.com/tu-usuario/tu-repositorio`
2. Comprueba que se ven todos tus archivos (`main.py`, `report.md`, etc.)
3. Haz clic en `report.md` para ver su contenido y verificar que el texto se ve bien
4. Haz clic en `main.py` para ver tu código

Si todo se ve correctamente, tu repositorio está listo para la entrega.

---

## 9. Crear el Issue de entrega

El **Issue** es el mecanismo oficial de entrega. Cuando lo crees, un sistema automático verificará tu repositorio y publicará un comentario con los resultados en pocos minutos.

1. Ve al repositorio del curso: **https://github.com/matcom/programming**
2. Haz clic en la pestaña **Issues**
3. Haz clic en el botón verde **New issue**
4. Verás una lista de plantillas. Selecciona **"Proyecto I - Curso 25-26"** haciendo clic en **Get started**
5. Rellena el formulario:
   - **Nombre:** Tu nombre completo
   - **Grupo:** Tu grupo (ej: C-111)
   - **Repositorio:** La URL completa de tu repositorio (ej: `https://github.com/tu-usuario/tu-proyecto`)
   - **Descripción:** Una o dos frases sobre lo que hace tu proyecto
6. Haz clic en **Submit new issue**

En unos minutos, un bot publicará un comentario automático con los resultados de la verificación. Verás algo como:

```
✅ Repositorio accesible
✅ report.md — 2,341 palabras
✅ Archivos Python (3, 180 líneas)
✅ main.py encontrado
✅ El proyecto se ejecuta
```

Si algún punto sale con ❌, corrígelo, haz push de los cambios, y comenta en el Issue mencionando que actualizaste el repositorio.

> **Importante:** la verificación automática es orientativa. El profesor revisará tu proyecto en detalle. Un resultado verde no garantiza una nota perfecta, pero un resultado rojo sí indica que hay algo que corregir.

---

## Preguntas frecuentes

**¿Puedo hacer cambios después de crear el Issue?**
Sí. Haz push de los cambios a tu repositorio normalmente. La verificación automática se ejecutó solo al crear el Issue, pero el profesor verá el estado más reciente de tu repo.

**¿Mi repositorio tiene que ser público?**
Sí, para que el verificador automático pueda clonarlo. Si tienes objeciones, habla con el profesor.

**¿Qué pasa si mi programa necesita entrada del usuario?**
No hay problema. El verificador intentará ejecutarlo y si se queda esperando input, lo tomará como que el programa arranca correctamente.

**¿Puedo usar librerías externas?**
Sí, siempre que las declares en `requirements.txt` o `pyproject.toml`.

**El verificador dice que no encontró report.md. ¿Qué hago?**
Verifica que el archivo se llama exactamente `report.md` (en minúsculas) y que está en la raíz del repositorio, no dentro de una carpeta.

---

## Resumen rápido

```
1. GitHub account       → https://github.com/signup
2. Crear repo           → github.com → New repository → Public
3. Instalar git         → sudo apt install git
4. Autenticarte         → sudo apt install gh && gh auth login
5. Clonar               → git clone <url> && code .
6. Crear archivos       → main.py + report.md (≥2000 palabras)
7. Commit y push        → git add . && git commit -m "..." && git push
8. Verificar            → github.com/tu-usuario/tu-repo
9. Entregar             → github.com/matcom/programming → Issues → New issue
```
