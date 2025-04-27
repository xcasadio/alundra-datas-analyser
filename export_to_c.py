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
from ghidra.program.model.data import Array
import os, struct

# === Setup output ===
base_dir = os.path.join(os.getcwd(), "export_project")
if not os.path.exists(base_dir):
    os.makedirs(base_dir)

f_globals_h = FileWriter(os.path.join(base_dir, "globals.h"))
f_globals_c = FileWriter(os.path.join(base_dir, "globals.c"))
f_functions  = FileWriter(os.path.join(base_dir, "functions.c"))

# === Export globals (globals.h) ===
memory = currentProgram.getMemory()
symbolTable = currentProgram.getSymbolTable()
symbols = symbolTable.getAllSymbols(True)

def read_value_bytes(addr, size):
    bytes = bytearray(size)
    try:
        memory.getBytes(addr, bytes)
        return bytes
    except:
        return None

def format_value(bytes, datatype):
    if not bytes:
        return None
    try:
        if datatype.lower() in ["int", "undefined4"]:
            return str(struct.unpack("<i", bytes[:4])[0])
        elif datatype.lower() in ["short", "undefined2"]:
            return str(struct.unpack("<h", bytes[:2])[0])
        elif datatype.lower() in ["char", "undefined1"]:
            return str(struct.unpack("<b", bytes[:1])[0])
        elif datatype.lower() == "float":
            return str(struct.unpack("<f", bytes[:4])[0])
    except:
        return None
    return None

for sym in symbols:
    if sym.getSymbolType() in [SymbolType.GLOBAL, SymbolType.LABEL]:
        var_name = sym.getName()

        if var_name.lower().startswith("switchdata") or var_name.startswith("PTR_case"):
            continue

        data = getDataAt(sym.getAddress())
        if not data:
            continue

        try:
            data_type = data.getDataType()
            type_name = data_type.getDisplayName()
            address = sym.getAddress().getOffset()

            # base type
            try:
                base_type = data_type.getBaseDataType()
                base_type_name = base_type.getName().lower()
            except:
                base_type_name = data_type.getName().lower()

            if base_type_name in ["string", "pointer"]:
                continue

            if isinstance(data_type, Array):
                elem_type = data_type.getDataType().getDisplayName()
                count = data_type.getNumElements()
                values = []

                for i in range(count):
                    try:
                        comp = data.getComponent(i)
                        val = comp.getValue()
                        if isinstance(val, (int, long, float)):
                            values.append(str(val))
                        else:
                            values.append("0")
                    except:
                        values.append("0")

                f_globals_c.write("{} {}[{}] = {{{}}}; // 0x{:08X}\n".format(
                    elem_type, var_name, count, ", ".join(values), address))

            else:
                value = data.getValue()
                if isinstance(value, (int, long, float)):
                    f_globals_c.write("{} {} = {}; // {:08X}\n".format(type_name, var_name, value, address))
                else:
                    f_globals_c.write("{} {}; // {:08X}\n".format(type_name, var_name, address))

        except Exception as e:
            print("Erreur sur", var_name, ":", str(e))
            pass


f_globals_h.close()
f_globals_c.close()

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
            f_functions.write("// {:08X}".format(func_addr))
            f_functions.write(code)

    except Exception as e:
        print("Error", var_name, ":", str(e))
        pass
    
f_functions.close()

print("[✓] Export complete in folder: {}".format(base_dir))
