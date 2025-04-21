# -*- coding: utf-8 -*-
#@author Xavier Casadio
#@category Export
#@keybinding
#@menupath
#@toolbar

from java.io import FileWriter
from ghidra.app.decompiler import DecompInterface
from ghidra.util.task import ConsoleTaskMonitor
from ghidra.program.model.symbol import SymbolType
from ghidra.program.model.data import Pointer, StringDataType
import os

# === Setup output ===
base_dir = os.path.join(os.getcwd(), "export_project")
if not os.path.exists(base_dir):
    os.makedirs(base_dir)

f_globals = FileWriter(os.path.join(base_dir, "globals.h"))
f_functions = FileWriter(os.path.join(base_dir, "functions.c"))

# === Export globals (globals.h), excluding type "string" and all pointers ===
symbolTable = currentProgram.getSymbolTable()
symbols = symbolTable.getAllSymbols(True)

for sym in symbols:
    if sym.getSymbolType() in [SymbolType.GLOBAL, SymbolType.LABEL]:
        var_name = sym.getName()

        # ❌ Filtrage par nom
        if var_name.lower().startswith("switchdata") or var_name.startswith("PTR_case"):
            continue

        data = getDataAt(sym.getAddress())
        if data:
            try:
                data_type = data.getDataType()
                type_name = data_type.getDisplayName()

                # base type name (string fallback)
                try:
                    base_type = data_type.getBaseDataType()
                    base_type_name = base_type.getName().lower()
                except:
                    base_type_name = data_type.getName().lower()

                # ❌ Exclure si le type est "string" ou "pointer"
                if base_type_name in ["string", "pointer"]:
                    continue

                # ✅ Sinon, on l’exporte
                f_globals.write("extern {} {};\n".format(type_name, var_name))

            except:
                pass


f_globals.close()

# === Export functions (functions.c) ===
f_functions.write("#include \"types.h\"\n")
f_functions.write("#include \"globals.h\"\n\n")

monitor = ConsoleTaskMonitor()
decomp = DecompInterface()
decomp.openProgram(currentProgram)

functionManager = currentProgram.getFunctionManager()
functions = functionManager.getFunctions(True)

for func in functions:
    if func.isExternal() or func.getBody().isEmpty():
        continue
    if func.getName().startswith("gte_"):
        continue

    res = decomp.decompileFunction(func, 60, monitor)
    if res and res.getDecompiledFunction():
        code = res.getDecompiledFunction().getC()
        func_addr = func.getEntryPoint().getOffset()
        f_functions.write("// Function @0x{:08X}".format(func_addr))
        f_functions.write(code + "\n")

f_functions.close()

print("[✓] Export complete in folder: {}".format(base_dir))
