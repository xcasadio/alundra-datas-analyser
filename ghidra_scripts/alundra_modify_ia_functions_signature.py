# -*- coding: utf-8 -*-
#@author Xavier Casadio
#@category Alundra
#@keybinding
#@menupath
#@toolbar

from ghidra.program.model.data import PointerDataType
from ghidra.program.model.data import VoidDataType
from ghidra.program.model.symbol import SourceType
from ghidra.program.model.listing import ParameterImpl, Function
from ghidra.util.exception import DuplicateNameException, InvalidInputException

# List of function addresses to update
target_function_addresses = [
    "0x8006174c",
	"0x80061758",
	"0x80061764",
	"0x800617b8",
	"0x800617c4",
	"0x800617d0",
	"0x800617dc",
	"0x800617e8",
	"0x800617f4",
	"0x80061808",
	"0x80061814",
	"0x80061820",
	"0x80061888",
	"0x8006191c",
	"0x80061930",
	"0x80061998",
	"0x800619a8",
	"0x800619c0",
	"0x800619d0",
	"0x800619dc",
	"0x800619e8",
	"0x80061a6c",
	"0x80065ed4",
	"0x80066250",
	"0x800665a0",
	"0x80066984",
	"0x80066bf8",
	"0x80066f38",
	"0x80067138",
	"0x8006790c",
	"0x80067d98",
	"0x80068154",
	"0x80068930",
	"0x80068cc8",
	"0x80069684",
	"0x800699c4",
	"0x80069c84",
	"0x80069f44",
	"0x8006a29c",
	"0x8006a564",
	"0x8006a974",
	"0x8006abb0",
	"0x8006b234",
	"0x8006b510",
	"0x8006b848",
	"0x8006b8cc",
	"0x8006b8d4",
	"0x8006bd30",
	"0x8006c100",
	"0x8006c5cc",
	"0x8006ca40",
	"0x8006ce08",
	"0x8006d550",
	"0x8006d998",
	"0x8006de68",
	"0x8006d998",
	"0x8006e2d8",
	"0x8006e83c",
	"0x8006e89c",
	"0x8006eb9c",
	"0x8006fc7c",
	"0x80070598",
	"0x80070c40",
	"0x8006f8e4",
	"0x80071164",
	"0x80071bf4",
	"0x8007252c",
	"0x80072728",
	"0x80073cfc",
	"0x80074d00",
	"0x80075a3c",
	"0x80076da0",
	"0x80077734",
	"0x80078a5c",
	"0x80078b54",
	"0x80078e34",
	"0x80079b14",
	"0x8007a2f8",
	"0x8007a4a8",
	"0x8007a4b0",
	"0x8007a680",
	"0x8007a8a0",
	"0x8007a958",
	"0x8007a978",
	"0x8007ac60",
	"0x8007addc",
	"0x8007af20",
	"0x8007b04c",
	"0x8007b1f0",
	"0x8007b3c4",
	"0x8007b6ec",
	"0x8007b7b0",
	"0x8007b834",
	"0x8007b998",
	"0x8007bb30",
	"0x8007bb9c",
	"0x8007bd8c",
	"0x8007c024",
	"0x80071134",
	"0x8007c768",
	"0x8006f860",
	"0x800756ec",
	"0x800749a4",
	"0x80074ae8",
	"0x8007763c",
	"0x8007d554",
	"0x80073728",
	"0x80079950",
	"0x80072680",
	"0x80061d14",
	"0x80061eb8",
	"0x80062bc0",
	"0x800637d8",
	"0x80063cb4",
	"0x80063db4",
	"0x8007c0d8",
	"0x80064294",
	"0x800647b0",
	"0x80064884",
	"0x80064d90",
	"0x80065100",
	"0x80065204",
	"0x80065750",
	"0x80065b0c",
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
	"0x8007ed10",
	"0x8007ed10",
	"0x8007ed30",
	"0x8007eda0",
	"0x8007ee04",
	"0x8007ee68",
	"0x8007eef0",
	"0x8007ef10",
	"0x8007ef30",
	"0x8007ef50",
	"0x8007f23c",
	"0x8007f25c",
	"0x8007f27c",
	"0x8007f30c",
	"0x8007f378",
	"0x8007f3b0",
	"0x8007f3e8",
	"0x8007f420",
	"0x8007f658",
	"0x8007f690",
	"0x8007f6c8",
	"0x8007f7cc",
	"0x8007f878",
	"0x8007f8ac",
	"0x8007f974",
	"0x8007faa0",
	"0x8007fac0",
	"0x8007fb38",
	"0x8007fb58"
	]

STRUCT_PATH_PREFIX = "/auto_structs/"

def findDataTypeByName(name):
    full_path = STRUCT_PATH_PREFIX + name
    return currentProgram.getDataTypeManager().getDataType(full_path)

# Retrieve the Entity structure pointer
entity_type = findDataTypeByName("Entity")
if entity_type is None:
    print("Error: Could not find 'Entity' structure in the Data Type Manager.")
    exit()

entity_ptr = PointerDataType(entity_type)

# Retrieve the void return type
void_type = VoidDataType.dataType

# Process each function
for addr_str in target_function_addresses:
    addr = toAddr(addr_str)
    func = getFunctionAt(addr)

    if func is None:
        print("No function found at address {}.".format(addr_str))
        continue

    try:
        # Rename function if necessary
        current_name = func.getName()
        if not current_name.startswith("AI_"):
            new_name = "AI_" + current_name
            func.setName(new_name, SourceType.USER_DEFINED)
            print("Renamed function at address {} to '{}'.".format(addr_str, new_name))
        else:
            new_name = current_name
            print("Function at address {} already starts with 'AI_'.".format(addr_str))

        # Set return type to void
        func.setReturnType(void_type, SourceType.USER_DEFINED)

        # Set parameters to (Entity *entity)
        param_list = [
            ParameterImpl("entity", entity_ptr, currentProgram)
        ]
        func.replaceParameters(Function.FunctionUpdateType.DYNAMIC_STORAGE_ALL_PARAMS, True, SourceType.USER_DEFINED, param_list)

        print("Updated signature for function '{}' at address {}.".format(new_name, addr_str))

    except (DuplicateNameException, InvalidInputException) as e:
        print("Error updating function at address {}: {}".format(addr_str, e))

print("Done.")
