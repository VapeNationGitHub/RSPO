.PHONY: compile run tests gen rmdb

TARGET=RSPO.exe
VAR=Debug
TRM="TERM=xterm XBUILD_COLORS=disable"
BUILDFLAGS=/property:Configuration="Debug" /verbosity:normal /nologo
#BUILD=msbuild $(BUILDLFAGS)
#BUILD=dotnet build $(BUILDLFAGS)
BUILD=xbuild $(BUILDLFAGS)
RUNNERVER=2.3.1
RUNTIME=net452
CODEGEN="/home/eugeneai/projects/code/RSPO/RSPO/packages/BrightstarDB.CodeGeneration.Console/tools/BrightstarDB.CodeGeneration.Console.exe"
GENNS="RSPO"

MONONODEBUG=mono
MONO=$(MONONODEBUG)
#MONO=mono --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555

compile: export TERM=xterm
compile: export XBUILD_COLORS=disable
compile:
	$(BUILD)

run: export TERM=xterm
run: export MONO_COLORS=disable
run: compile
	cd bin/$(VAR) && $(MONO) $(TARGET)

tests: export TERM=xterm
tests: export MONO_COLORS=disable
tests: compile
	$(MONO) packages/xunit.runner.console/tools/$(RUNTIME)/xunit.console.exe bin/$(VAR)/RSPO.exe -nologo -nocolor

# FIXME: This does not work
gen: export TERM=xterm
gen: export MONO_COLORS=disable
gen:
	# $(MONONODEBUG) $(CODEGEN) /EntityContext:MyEntityContext /Language:CS /InternalEntityClasses ../RSPO.sln RSPO Output.cs
	$(MONO) $(CODEGEN) -CN MyEntityContext -IE -O Output.cs -C RSPO -S ../RSPO.sln
	# $(MONONODEBUG) $(CODEGEN) ../RSPO.sln RSPO Output.cs -CN MyEntityContext -IE

rmdb:
	rm -rf bin/Debug/RSPO
