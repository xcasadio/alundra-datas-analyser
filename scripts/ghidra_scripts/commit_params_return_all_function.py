# -*- coding: utf-8 -*-
#
#@author Xavier Casadio
#@category Alundra
#@keybinding 
#@menupath 
#@toolbar

from ghidra.util.task import ConsoleTaskMonitor
from ghidra.app.decompiler import DecompInterface
from ghidra.program.model.symbol import SourceType
from ghidra.program.model.pcode import HighFunctionDBUtil
from ghidra.program.model.pcode.HighFunctionDBUtil import ReturnCommitOption

decomp_interface = DecompInterface()
decomp_interface.openProgram(currentProgram)

function_manager = currentProgram.getFunctionManager()
functions = function_manager.getFunctions(True)

monitor = ConsoleTaskMonitor()

for func in functions:
    try:
        decomp_result = decomp_interface.decompileFunction(func, 60, monitor)
        if not decomp_result.decompileCompleted():
            print("Décompilation échouée pour la fonction : {}".format(func.getName()))
            continue

        high_func = decomp_result.getHighFunction()

        HighFunctionDBUtil.commitParamsToDatabase(
            high_func,
            True,  # Utiliser les types de données
            ReturnCommitOption.COMMIT,  # Option de validation du type de retour
            SourceType.USER_DEFINED
        )

        print("Paramers and return validated for the function : {}".format(func.getName()))

    except Exception as e:
        print("Error with function {}: {}".format(func.getName(), e))

