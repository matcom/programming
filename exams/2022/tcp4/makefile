build:
	dotnet build

zip:
	rm C2XX-NombreApellidoApellido.zip
	zip -j C2XX-NombreApellidoApellido.zip trees/Trees.cs

archive:
	git archive HEAD . > tcp4.zip

pdf:
	pandoc Readme.md -o Readme.pdf
