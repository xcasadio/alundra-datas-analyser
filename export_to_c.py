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
from ghidra.program.model.data import Pointer, StringDataType, Array
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

        if var_name.lower().startswith("switchdata") or var_name.startswith("PTR_case"):
            continue

        try:
            data = getDataAt(sym.getAddress())
            if data is None:
                continue

            data_type = data.getDataType()
            address = sym.getAddress().getOffset()

            # base type
            try:
                base_type = data_type.getBaseDataType()
                base_type_name = base_type.getName().lower()
            except:
                base_type = data_type
                base_type_name = data_type.getName().lower()

            if base_type_name in ["string", "pointer"]:
                continue

            if isinstance(data_type, Array):
                element_type = data_type.getDataType().getDisplayName()
                count = data_type.getNumElements()
                f_globals.write("extern {} {}[{}]; // {:08X}\n".format(
                    element_type, var_name, count, address))
            else:
                type_name = data_type.getDisplayName()
                f_globals.write("extern {} {}; // {:08X}\n".format(
                    type_name, var_name, address))

        except Exception as e:
            print("Error", var_name, ":", str(e))
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
    try:
        if func.isExternal() or func.getBody().isEmpty():
            continue
        if func.getName().startswith("gte_"):
            continue

        res = decomp.decompileFunction(func, 60, monitor)
        if res and res.getDecompiledFunction():
            code = res.getDecompiledFunction().getC()
            func_addr = func.getEntryPoint().getOffset()
            f_functions.write("//{:08X}".format(func_addr))
            f_functions.write(code)

    except Exception as e:
        print("Error", var_name, ":", str(e))
        pass
    
f_functions.close()

print("[✓] Export complete in folder: {}".format(base_dir))
