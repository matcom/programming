test:
	dotnet run --project exam

build:
	dotnet build

zip:
	rm C2XX-NombreApellidoApellido.zip | echo "Nada que eliminar"
	zip -j C2XX-NombreApellidoApellido.zip exam/Exam.cs

archive:
	git archive HEAD . -o filesystem.zip

pdf:
	pandoc Readme.md -o Readme.pdf
