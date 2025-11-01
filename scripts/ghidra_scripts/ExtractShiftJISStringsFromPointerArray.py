#@category Alundra.Export
#@author Xavier Casadio
# -*- coding: utf-8 -*-

# Script: ExtractShiftJISStringsFromPointerArray.py
# Purpose:
#   Read a fixed array of 32-bit pointers to Shift-JIS strings at a known address,
#   dereference each pointer, read the null-terminated bytes, decode as Shift-JIS,
#   and export results to a CSV file. Also prints a short preview to the console.
#
# Configuration:
#   BASE_ADDR_HEX : start address of the pointer array (e.g., 0x8009870c)
#   COUNT         : number of entries (e.g., 512)
#   PTR_SIZE      : pointer size in bytes (4 for PS1)
#   MAX_STR_LEN   : safety cap for string length in bytes
#
# Notes:
#   - No data types are created/modified in the listing; this only reads memory.
#   - Strings are decoded using Shift-JIS; undecodable bytes are replaced.
#   - CSV is written next to the current project dir unless you change OUTPUT_PATH.

import os

from ghidra.program.model.mem import MemoryAccessException

# ---- Configuration ----
BASE_ADDR_HEX = "0x8009870c"
COUNT = 512
PTR_SIZE = 4
MAX_STR_LEN = 4096  # bytes

def to_hex(n):
    return "0x{0:08x}".format(n & 0xffffffff)

def to_address(hex_str):
    """Return a Ghidra Address from a hex string (with or without 0x)."""
    s = str(hex_str).strip().lower()
    if not s.startswith("0x"):
        s = "0x" + s
    return toAddr(s)

def read_u32(addr):
    """Read a 32-bit unsigned value at addr using program endianness."""
    mem = currentProgram.getMemory()
    try:
        return mem.getInt(addr) & 0xffffffff  # mask to unsigned
    except MemoryAccessException as e:
        raise RuntimeError("Cannot read u32 at {}: {}".format(addr, e))

def memory_contains(address):
    """Check whether memory contains the given address (at least 1 byte)."""
    try:
        return currentProgram.getMemory().contains(address)
    except:
        return False

def read_c_string(addr, max_len=MAX_STR_LEN):
    """Read a null-terminated byte string from memory starting at addr; decode as Shift-JIS."""
    mem = currentProgram.getMemory()
    out = []
    cur = addr
    for _ in range(max_len):
        try:
            b = mem.getByte(cur)
        except MemoryAccessException:
            break
        b = (b + 256) % 256  # signed byte -> 0..255
        if b == 0:
            break
        out.append(b)
        cur = cur.add(1)

    # Decode bytes as Shift-JIS
    try:
        text = bytearray(out).decode('shift_jis', 'replace')
    except Exception:
        # Java fallback
        from java.nio.charset import Charset
        from java.nio import ByteBuffer
        bb = ByteBuffer.wrap(bytearray(out))
        text = Charset.forName("Shift_JIS").decode(bb).toString()

    return bytearray(out), text

def choose_output_path(default_name="extracted_strings.csv"):
    """Prompt user for an output CSV path; return absolute path or None if cancelled."""
    f = askFile("Choose CSV output path", "Save")
    if f is None:
        return None
    # If a directory was chosen, append default name; otherwise use the file path
    if os.path.isdir(f.getAbsolutePath()):
        return os.path.join(f.getAbsolutePath(), default_name)
    return f.getAbsolutePath()

def main():
    base = to_address(BASE_ADDR_HEX)
    print("[INFO] Pointer array base: {}  count: {}".format(base, COUNT))

    rows = []
    preview_limit = 10

    for i in range(COUNT):
        ptr_addr = base.add(i * PTR_SIZE)

        # Read pointer value
        try:
            ptr_val = read_u32(ptr_addr)
        except RuntimeError as e:
            print("[WARN] [{}] Failed to read pointer at {}: {}".format(i, ptr_addr, e))
            rows.append((i, str(ptr_addr), "", 0, ""))
            continue

        if ptr_val == 0:
            rows.append((i, str(ptr_addr), "", 0, ""))
            continue

        # Convert pointer value to Address
        try:
            str_addr = toAddr(to_hex(ptr_val))
        except Exception as e:
            print("[WARN] [{}] Invalid pointer value {} at {}: {}".format(i, to_hex(ptr_val), ptr_addr, e))
            rows.append((i, str(ptr_addr), to_hex(ptr_val), 0, ""))
            continue

        if not memory_contains(str_addr):
            print("[WARN] [{}] Target address not in memory: {}".format(i, str_addr))
            rows.append((i, str(ptr_addr), to_hex(ptr_val), 0, ""))
            continue

        raw_bytes, text = read_c_string(str_addr, MAX_STR_LEN)
        rows.append((i, str(ptr_addr), to_hex(ptr_val), len(raw_bytes), text))

        if i < preview_limit:
            # Using unicode-safe print; Jython handles this fine
            print(u"[{}] ptr@{} -> {}  len={}  text='{}'".format(
                i, ptr_addr, str_addr, len(raw_bytes), text))

    # Ask for output file
    out_path = choose_output_path("extracted_strings_8009870c.csv")
    if not out_path:
        print("[INFO] Save cancelled; skipping CSV write.")
        print("[DONE] Extracted {} entries.".format(len(rows)))
        return

    # Write CSV (UTF-8)
    try:
        # Use binary mode and encode lines manually (robust on Jython/Windows)
        with open(out_path, "wb") as f:
            header = u"index,pointer_address,string_address,length_bytes,text\n"
            f.write(header.encode("utf-8"))
            for idx, paddr, saddr, length, text in rows:
                # Escape quotes inside CSV field
                txt = (text or u"").replace(u"\"", u"\"\"")
                line = u'{},{},{},{},\"{}\"\n'.format(idx, paddr, saddr, length, txt)
                f.write(line.encode("utf-8"))
        print("[OK] CSV written to: {}".format(out_path))
    except Exception as e:
        print("[ERROR] Failed to write CSV '{}': {}".format(out_path, e))

    print("[DONE] Extracted {} entries.".format(len(rows)))

if __name__ == "__main__":
    main()
