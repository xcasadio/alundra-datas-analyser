#Update ia functions signatures based on their memory addresses
#@author Xavier Casadio
#@category Alundra
#@keybinding 
#@menupath 
#@toolbar 

from ghidra.program.model.data import PointerDataType
from ghidra.program.model.symbol import SourceType
from ghidra.program.model.listing import ParameterImpl
from ghidra.program.model.listing import Function

# List of function addresses to update
target_function_addresses = [
    "0x8007d9a4",
    "0x8007da08",
    "0x8007db38",
    "0x8007dbe0",
    "0x8007dcd8",
    "0x8007dd3c",
    "0x8007dda0",
    "0x8007de04",
    "0x8007de68",
    "0x8007dee8",
    "0x8007df4c",
    "0x8007e074",
    "0x8007e0d8",
    "0x8007e114",
    "0x8007e140",
    "0x8007e1c4",
    "0x8007e228",
    "0x8007e2a0",
    "0x8007e304",
    "0x8007e424",
    "0x8007e548",
    "0x8007e5b0",
    "0x8007e628",
    "0x8007e68c",
    "0x8007e694",
    "0x8007e704",
    "0x8007e754",
    "0x8007e790",
    "0x8007e79c",
    "0x8007e7e4",
    "0x8007e828",
    "0x8007e86c",
    "0x8007e8ac",
    "0x8007e8f0",
    "0x8007e994",
    "0x8007e9ac",
    "0x8007e9e8",
    "0x8007ea24",
    "0x8007ea84",
    "0x8007eb1c",
    "0x8007eb58",
    "0x8007eba8",
    "0x8007ebf0",
    "0x8007ec60",
    "0x8007ec9c",
]

STRUCT_PATH_PREFIX = "/auto_structs/"

def findDataTypeByName(name):
    full_path = STRUCT_PATH_PREFIX + name
    return currentProgram.getDataTypeManager().getDataType(full_path)

# Get pointer types
entity_type = findDataTypeByName("Entity")

if entity_type is None:
    print("Error: Could not find Entity in Data Type Manager.")
    exit()

entity_ptr = PointerDataType(entity_type)

# Get int return type
int_type = currentProgram.getDataTypeManager().getDataType("/int")
if int_type is None:
    print("Error: Could not find 'int' type.")
    exit()

# Process each function
for addr_str in target_function_addresses:
    addr = toAddr(addr_str)
    func = getFunctionAt(addr)

    if func is None:
        print("No function found at address {}.".format(addr_str))
        continue

    try:
        param_list = [
            ParameterImpl("entity", entity_ptr, currentProgram)
        ]
        func.replaceParameters(Function.FunctionUpdateType.DYNAMIC_STORAGE_ALL_PARAMS, True, SourceType.USER_DEFINED, param_list)
        print("Updated signature for function at {}.".format(addr_str))

    except Exception as e:
        print("Error updating function at {}: {}".format(addr_str, e))

print("Done.")
