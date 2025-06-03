# -*- coding: utf-8 -*-
#rename all objects in a fonction by chatGPT
#@author xavier casadio
#@category ReverseEngineering
#@keybinding 
#@menupath 
#@toolbar 
#@runtime Jython

import json
import urllib2
from ghidra.util.task import ConsoleTaskMonitor
from ghidra.program.model.symbol import SourceType
from ghidra.app.decompiler import DecompInterface
from java.net import URL
from java.io import BufferedReader, InputStreamReader, OutputStreamWriter
from javax.net.ssl import HttpsURLConnection

# ===== CONFIGURATION =====
OPENAI_API_KEY = "your_key"
MODEL = "gpt-4"
TEMPERATURE = 0.3

# ===== UTILS =====
def get_decompiled_code(func):
    decomp = DecompInterface()
    decomp.openProgram(currentProgram)
    results = decomp.decompileFunction(func, 60, ConsoleTaskMonitor())
    if results.decompileCompleted():
        return results.getDecompiledFunction().getC()
    return "// Failed to decompile"

def get_disassembly_code(func):
    listing = currentProgram.getListing()
    code = ""
    for instr in listing.getInstructions(func.getBody(), True):
        code += instr.toString() + "\n"
    return code

def call_chatgpt_with_function(decompiled, disasm):
    prompt = (
        "You're a reverse engineering assistant. Your task is to improve the readability of this function "
        "by renaming:\n- the function name\n- all called functions\n- local and global variables\n"
        "- structure fields\n- jump labels.\n\n"
        "You MUST return ONLY a JSON object with the following structure:\n"
        "{\n"
        "  \"function_name\": \"NewFunctionName\",\n"
        "  \"called_functions\": { \"oldFunc\": \"newFunc\" },\n"
        "  \"local_variables\": { \"oldVar\": \"newVar\" },\n"
        "  \"global_variables\": { \"oldGlobal\": \"newGlobal\" },\n"
        "  \"structure_fields\": { \"StructName\": { \"oldField\": \"newField\" } },\n"
        "  \"labels\": { \"oldLabel\": \"newLabel\" }\n"
        "}\n\n"
        "Here is the function in C (decompiled):\n"
        "```c\n" + decompiled + "\n```\n\n"
        "And here is the disassembly:\n"
        "```asm\n" + disasm + "\n```"
    )

    payload = json.dumps({
        "model": "gpt-3.5-turbo",
        "messages": [{"role": "user", "content": prompt}],
        "temperature": 0.3
    })

    try:
        url = URL("https://api.openai.com/v1/chat/completions")
        conn = url.openConnection()
        conn.setRequestMethod("POST")
        conn.setRequestProperty("Authorization", "Bearer " + OPENAI_API_KEY)
        conn.setRequestProperty("Content-Type", "application/json")
        conn.setDoOutput(True)

        writer = OutputStreamWriter(conn.getOutputStream(), "UTF-8")
        writer.write(payload)
        writer.flush()
        writer.close()

        response_code = conn.getResponseCode()
        if response_code != 200:
            print("HTTP error:", response_code)
            print("Message:", conn.getResponseMessage())
            return None

        reader = BufferedReader(InputStreamReader(conn.getInputStream(), "UTF-8"))
        response_text = ""
        line = reader.readLine()
        while line:
            response_text += line
            line = reader.readLine()
        reader.close()

        parsed = json.loads(response_text)
        return json.loads(parsed["choices"][0]["message"]["content"])

    except Exception as e:
        print("Erreur pendant l'appel à ChatGPT :")
        print(e)
        return None

def rename_all(func, mapping):
    if "function_name" in mapping:
        new_name = mapping["function_name"]
        if new_name != func.getName():
            func.setName(new_name, SourceType.USER_DEFINED)

    if "called_functions" in mapping:
        for old, new in mapping["called_functions"].items():
            funcs = getGlobalFunctions(old)
            if funcs:
                funcs[0].setName(new, SourceType.USER_DEFINED)

    if "local_variables" in mapping:
        try:
            decomp = DecompInterface()
            decomp.openProgram(currentProgram)
            results = decomp.decompileFunction(func, 60, ConsoleTaskMonitor())
            if results.decompileCompleted():
                high_func = results.getHighFunction()
                lmap = high_func.getLocalSymbolMap()
                for sym in lmap.getSymbols():
                    old_name = sym.getName()
                    if old_name in mapping["local_variables"]:
                        sym.setName(mapping["local_variables"][old_name])
        except Exception as e:
            print("Erreur lors du renommage des variables locales:", e)

    if "global_variables" in mapping:
        for old, new in mapping["global_variables"].items():
            syms = currentProgram.getSymbolTable().getSymbols(old)
            for sym in syms:
                if sym.isGlobal():
                    sym.setName(new, SourceType.USER_DEFINED)

    if "labels" in mapping:
        for old, new in mapping["labels"].items():
            label = getSymbol(old)
            if label:
                label.setName(new, SourceType.USER_DEFINED)

    if "structure_fields" in mapping:
        dtm = currentProgram.getDataTypeManager()
        for struct_name, fields in mapping["structure_fields"].items():
            struct = dtm.getDataType("/Structures/" + struct_name)
            if struct:
                for i in range(struct.getNumComponents()):
                    comp = struct.getComponent(i)
                    old_field = comp.getFieldName()
                    if old_field in fields:
                        comp.setFieldName(fields[old_field])

# ===== MAIN SCRIPT =====
func = getFunctionContaining(currentAddress)

if not func:
    print("Place the cursor inside a function.")
else:
    print("[+] Analysing function: " + func.getName())
    decompiled_code = get_decompiled_code(func)
    disasm_code = get_disassembly_code(func)

    print("[+] Sending function to ChatGPT...")
    mapping = call_chatgpt_with_function(decompiled_code, disasm_code)

    if mapping:
        print("[+] Renaming identifiers...")
        rename_all(func, mapping)
        print("[✓] Done.")
    else:
        print("[-] Failed to get a valid response from ChatGPT.")

