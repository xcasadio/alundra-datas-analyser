#Update function signatures based on their memory addresses
#@author Xavier Casadio
#@category Alundra
#@keybinding 
#@menupath 
#@toolbar 

from ghidra.program.model.data import PointerDataType, ByteDataType
from ghidra.program.model.symbol import SourceType
from ghidra.program.model.listing import ParameterImpl, Function
from ghidra.util.exception import DuplicateNameException, InvalidInputException

# List of function addresses to update
target_function_addresses = [
    "0x8003d158",
    "0x8003d158",
    "0x8003d17c",
    "0x8003d1a0",
    "0x8003d1d8",
    "0x8003d210",
    "0x8003d288",
    "0x8003d308",
    "0x8003d404",
    "0x8003d42c",
    "0x8003d44c",
    "0x8003d468",
    "0x8003d518",
    "0x8003d578",
    "0x8003d158",
    "0x8003d158",
    "0x8003d688",
    "0x8003d6a4",
    "0x8003d6c0",
    "0x8003d158",
    "0x8003d6ec",
    "0x8003d710",
    "0x8003d774",
    "0x8003d78c",
    "0x8003d158",
    "0x8003d7a4",
    "0x8003d7b4",
    "0x8003d7d0",
    "0x8003d7fc",
    "0x8003d890",
    "0x8003d8d8",
    "0x8003d974",
    "0x8003d9bc",
    "0x8003da28",
    "0x8003da70",
    "0x8003db28",
    "0x8003db70",
    "0x8003db7c",
    "0x8003dba8",
    "0x8003dbd4",
    "0x8003dc24",
    "0x8003dc3c",
    "0x8003dc54",
    "0x8003dc6c",
    "0x8003dc84",
    "0x8003dcc4",
    "0x8003dd00",
    "0x8003dd8c",
    "0x8003dddc",
    "0x8003de6c",
    "0x8003defc",
    "0x8003df74",
    "0x8003e128",
    "0x8003e2dc",
    "0x8003e35c",
    "0x8003e3dc",
    "0x8003e424",
    "0x8003e464",
    "0x8003e484",
    "0x8003e4b4",
    "0x8003e558",
    "0x8003e64c",
    "0x8003e708",
    "0x8003e734",
    "0x8003e7b8",
    "0x8003e7e4",
    "0x8003e808",
    "0x8003e81c",
    "0x8003e88c",
    "0x8003e954",
    "0x8003e96c",
    "0x8003e984",
    "0x8003e9b0",
    "0x8003e9dc",
    "0x8003e9ec",
    "0x8003ea14",
    "0x8003ea3c",
    "0x8003ea68",
    "0x8003ea88",
    "0x8003eab4",
    "0x8003ead4",
    "0x8003eb00",
    "0x8003eb20",
    "0x8003eb88",
    "0x8003ecbc",
    "0x8003ed5c",
    "0x8003edfc",
    "0x8003ee28",
    "0x8003ee5c",
    "0x8003ee8c",
    "0x8003eef4",
    "0x8003ef80",
    "0x8003f144",
    "0x8003f1a0",
    "0x8003f3f8",
    "0x8003f488",
    "0x8003f514",
    "0x8003f590",
    "0x8003f610",
    "0x8003f6c8",
    "0x8003f82c",
    "0x8003f868",
    "0x8003f878",
    "0x8003f8dc",
    "0x8003f908",
    "0x8003f94c",
    "0x8003f990",
    "0x8003f9d4",
    "0x8003f9e8",
    "0x8003f9fc",
    "0x8003fa10",
    "0x8003fa24",
    "0x8003fa3c",
    "0x8003fa58",
    "0x8003fa9c",
    "0x8003fac8",
    "0x8003faec",
    "0x8003fb10",
    "0x8003fb44",
    "0x8003fb8c",
    "0x8003fbd4",
    "0x8003fc74",
    "0x8003fd14",
    "0x8003fd24",
    "0x8003fd4c",
    "0x8003fd74",
    "0x8003fdf8",
    "0x8003fe7c",
    "0x8003fec8",
    "0x8003ff34",
    "0x8003ff84",
    "0x8003ffd4",
    "0x80040048",
    "0x8004011c",
    "0x80040194",
    "0x80040284",
    "0x8004033c",
    "0x80040438",
    "0x800404a8",
    "0x80040534",
    "0x80040598",
    "0x800405a8",
    "0x800405d4",
    "0x80040628",
    "0x80040680",
    "0x8004071c",
    "0x800407c0",
    "0x800409a8",
    "0x80040a2c",
    "0x80040a58",
    "0x80040a8c",
    "0x80040b00",
    "0x80040b68",
    "0x80040b78",
    "0x80040b88",
    "0x80040bb4",
    "0x80040c80",
    "0x80040d60",
    "0x80040e10",
    "0x80040f00",
    "0x80040fac",
    "0x80041098",
    "0x800410c8",
    "0x800410e8",
    "0x80041114",
    "0x80041144",
    "0x80041174",
    "0x80041200",
    "0x80041290",
    "0x800412c4",
    "0x80041344",
    "0x800414b4",
    "0x800414e0",
    "0x80041570",
    "0x800415e8",
    "0x80041628",
    "0x80041750",
    "0x8003f794",
    "0x800417cc", 
    "0x80041830", 
    "0x80041894", 
    "0x800418f8", 
    "0x80041988", 
    "0x80041a18", 
    "0x80041a44", 
    "0x80041a74", 
    "0x80041c00", 
    "0x80041c38", 
    "0x80041c6c", 
    "0x80041ca0", 
    "0x80041cdc", 
    "0x80041d18", 
    "0x80041d34", 
    "0x80041d6c", 
    "0x80041da8"
]

STRUCT_PATH_PREFIX = "/auto_structs/"

def findDataTypeByName(name):
    full_path = STRUCT_PATH_PREFIX + name
    return currentProgram.getDataTypeManager().getDataType(full_path)

# Retrieve data types
entity_type = findDataTypeByName("Entity")
event_program_state_type = findDataTypeByName("EventProgramState")

if entity_type is None or event_program_state_type is None:
    print("Error: Could not find 'Entity' or 'EventProgramState' in the Data Type Manager.")
    exit()

entity_ptr = PointerDataType(entity_type)
byte_ptr = PointerDataType(ByteDataType.dataType)
event_program_state_ptr = PointerDataType(event_program_state_type)

# Retrieve 'int' return type
int_type = currentProgram.getDataTypeManager().getDataType("/int")
if int_type is None:
    print("Error: Could not find the 'int' type.")
    exit()

# Process each function
for addr_str in target_function_addresses:
    addr = toAddr(addr_str)
    func = getFunctionAt(addr)

    if func is None:
        print("No function found at address {}.".format(addr_str))
        continue

    try:
        # Set return type
        func.setReturnType(int_type, SourceType.USER_DEFINED)

        # Set parameters
        param_list = [
            ParameterImpl("logicEntity", entity_ptr, currentProgram),
            ParameterImpl("ownerEntity", entity_ptr, currentProgram),
            ParameterImpl("variables", byte_ptr, currentProgram),
            ParameterImpl("eventProgramState", event_program_state_ptr, currentProgram)
        ]
        func.replaceParameters(Function.FunctionUpdateType.DYNAMIC_STORAGE_ALL_PARAMS, True, SourceType.USER_DEFINED, param_list)
        print("Updated signature for function at address {}.".format(addr_str))

        # Rename function if it doesn't start with 'Script_'
        current_name = func.getName()
        if not current_name.startswith("Script_"):
            new_name = "Script_" + current_name
            func.setName(new_name, SourceType.USER_DEFINED)
            print("Renamed function at address {} to '{}'.".format(addr_str, new_name))      

    except (DuplicateNameException, InvalidInputException) as e:
        print("Error updating function at address {}: {}".format(addr_str, e))

print("Done.")