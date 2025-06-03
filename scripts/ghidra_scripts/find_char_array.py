# -*- coding: utf-8 -*-
#@author Xavier Casadio
#@category Alundra
#@keybinding 
#@menupath 
#@toolbar 

# Script Ghidra: Automatically create 'char' arrays between consecutive labels in a memory range.

from ghidra.program.model.data import CharDataType
from ghidra.program.model.symbol import SymbolType
from ghidra.util import Msg

# --- Configuration ---
startAddr = toAddr(0x80024150)  # Start address
endAddr   = toAddr(0x80026330)  # End address
# -----------------------

symbolTable = currentProgram.getSymbolTable()
listing = currentProgram.getListing()

# Collect all labels in the specified range
symbols = []
iter = symbolTable.getSymbolIterator(True)
while iter.hasNext():
    sym = iter.next()
    addr = sym.getAddress()
    if startAddr <= addr <= endAddr:
        if sym.getSymbolType() == SymbolType.LABEL:
            symbols.append(sym)

if not symbols:
    Msg.error(None, "No labels found between %s and %s." % (startAddr, endAddr))
else:
    Msg.info(None, "%d labels found between %s and %s." % (len(symbols), startAddr, endAddr))

# Sort labels by address
symbols.sort(key=lambda s: s.getAddress().getOffset())

created_count = 0

for idx, symbol in enumerate(symbols):
    addr = symbol.getAddress()
    Msg.info(None, "Processing label %s at %s" % (symbol.getName(), addr))

    if idx + 1 < len(symbols):
        next_addr = symbols[idx + 1].getAddress()
    else:
        next_addr = endAddr

    size = next_addr.getOffset() - addr.getOffset()

    if size <= 0:
        Msg.error(None, "Invalid size between %s and %s, skipped." % (addr, next_addr))
        continue

    try:
        # Clear existing code or data
        clearListing(addr, next_addr)

        # Create an array of 'char'
        createArray(addr, CharDataType.dataType, size)

        created_count += 1
        Msg.info(None, "Created char array[%d] at %s (%s)" % (size, addr, symbol.getName()))
    except Exception as e:
        Msg.error(None, "Error at %s: %s" % (addr, str(e)))

Msg.info(None, "Done: %d arrays created." % created_count)
