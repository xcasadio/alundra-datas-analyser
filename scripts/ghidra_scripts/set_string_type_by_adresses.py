# -*- coding: utf-8 -*-
#Set some variables to string
#@author Xavier Casadio
#@category Alundra.Export
#@keybinding
#@menupath
#@toolbar


from ghidra.program.model.data import DataUtilities
from ghidra.program.model.data.DataUtilities import ClearDataMode

# Try to use AsciiStringDataType if available; fallback to StringDataType otherwise.
try:
    from ghidra.program.model.data import AsciiStringDataType
    STRING_DT = AsciiStringDataType()  # Preferred when available
except Exception:
    from ghidra.program.model.data import StringDataType
    STRING_DT = StringDataType()       # Works across older Ghidra versions

# ---- Edit this list ----
ADDRESSES = [
    "80021cf4",
"80021d04",
"80021d14",
"80021d20",
"80021d2c",
"80021d38",
"80021d4c",
"80021d60",
"80021d74",
"80021d84",
"80021d98",
"80021dac",
"80021db8",
"80021dc8",
"80021dd8",
"80021de8",
"80021df8",
"80021e08",
"80021e18",
"80021e28",
"80021e38",
"80021e48",
"80021e58",
"80021e68",
"80021e78",
"80021e80",
"80021e98",
"80021eac",
"80021ec0",
"80021ecc",
"80021edc",
"80021eec",
"80021efc",
"80021f0c",
"80021f20",
"80021f34",
"80021f48",
"80021f5c",
"80021f70",
"80021f84",
"80021f98",
"80021fac",
"80021fc0",
"80021fd4",
"80021fe8",
"80021ffc",
"80022010",
"80022024",
"80022038",
"8002204c",
"80022058",
"80022068",
"8002207c",
"8002208c",
"800220a0",
"800220b4",
"800220c8",
"800220dc",
"800220f0",
"80022104",
"80022118",
"8002212c",
"80022140",
"80022154",
"80022168",
"8002217c",
"80022190",
"800221a4",
"800221b8",
"800221cc",
"800221dc",
"800221f0",
"80022200",
"80022214",
"80022228",
"8002223c",
"80022250",
"80022264",
"80022278",
"80022288",
"8002229c",
"800222b0",
"800222c4",
"800222d8",
"800222ec",
"80022300",
"80022308",
"80022318",
"80022328",
"80022338",
"80022344",
"8002235c",
"80022374",
"80022388",
"80022398",
"800223a8",
"800223b8",
"800223c8",
"800223d8",
"800223e8",
"800223f4",
"800223fc",
"80022408",
"80022410",
"80022418",
"80022420",
"8002242c",
"80022438",
"80022440",
		 
]

def to_hex_addr(token):
    """Normalize a hex string (with or without 0x) and return a Ghidra Address."""
    s = token.strip().lower()
    if not s:
        return None
    if not s.startswith("0x"):
        s = "0x" + s
    return toAddr(s)

def set_ascii_string_at(addr):
    """Create an ASCII null-terminated string at the given address, clearing conflicts."""
    DataUtilities.createData(
        currentProgram,
        addr,
        STRING_DT,         
        -1,                             # auto length (until 0x00)
        False,                 
        ClearDataMode.CLEAR_ALL_CONFLICT_DATA
    )

def main():
    # Parse all addresses first
    parsed = []
    errors = []
    for tok in ADDRESSES:
        if not tok or not str(tok).strip():
            continue
        try:
            parsed.append(to_hex_addr(str(tok)))
        except Exception as e:
            errors.append((tok, str(e)))

    if not parsed:
        print("[ERROR] No valid addresses in ADDRESSES.")
        if errors:
            print("[INFO] Parse errors:")
            for tok, err in errors:
                print("  - {} ({})".format(tok, err))
        return

    tx = currentProgram.startTransaction("Set ASCII strings at fixed addresses ({})".format(len(parsed)))
    success = 0
    failure = 0
    try:
        for addr in parsed:
            try:
                set_ascii_string_at(addr)
                success += 1
                print("[OK] Created ASCII string at {}".format(addr))
            except Exception as e:
                failure += 1
                print("[ERROR] {} : {}".format(addr, e))

        if errors:
            print("\n[INFO] Ignored tokens during parsing:")
            for tok, err in errors:
                print("  - {} ({})".format(tok, err))
    finally:
        currentProgram.endTransaction(tx, True)

    print("\nDone. Success: {}, Failures: {}, Total valid: {}."
          .format(success, failure, len(parsed)))

if __name__ == "__main__":
    main()