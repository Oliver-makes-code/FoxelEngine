namespace Voxel.Common.Util.Collections;

public struct PalettedArray<TPaletteItem> where TPaletteItem : struct {
    public readonly int Size;
    private readonly List<TPaletteItem> Palette = [];
    private readonly Dictionary<TPaletteItem, int> PaletteIndeces = [];
    private int bitLength = 0;
    private byte[] backingArray;

    public TPaletteItem this[int index] {
        get => Palette[GetValue(backingArray, bitLength, index)];
        set {
            // If the palette contains the item, just write it
            if (PaletteIndeces.TryGetValue(value, out int palette)) {
                SetValue(backingArray, bitLength, index, palette);
                return;
            }
            // Add the palette item and write it
            AddPaletteItem(value);
            SetValue(backingArray, bitLength, index, PaletteIndeces[value]);
        }
    }

    public PalettedArray(int size) {
        Size = size;
        backingArray = [];
    }

    private static int CalculateBitLength(int paletteLength) {
        // Empty array, no indices
        if (paletteLength <= 1)
            return 0;
        
        // Get the minimum power of two bits needed to fir a number up to paletteLength 
        for (int i = 1; i < 32; i *= 2)
            if (paletteLength < (1 << i))
                return i;

        // The above loop only goes up to i<32, so it'll always be 32 here.
        return 32;
    }

    private static int GetValue(byte[] arr, int bitLength, int index) {
        // Empty palette can have only one possible value
        if (bitLength <= 1)
            return 0;
        // Bit length is less than a byte
        if (bitLength < 8) {
            // Calculate the bit mask
            int mask = (1 << bitLength) - 1;
            // Get the number of indices per byte
            int countPerByte = 8 / bitLength;
            // Calculate the byte index and offset
            (int calculatedIndex, int offset) = int.DivRem(index, countPerByte);
            // Get the byte
            int value = arr[calculatedIndex];
            // Get the amount to shift the mask by
            offset *= bitLength;
            // Mask the value
            value &= mask << offset;
            // Shift back the value
            return value >> offset;
        }
        // Bit length is more than a byte
        if (bitLength > 8) {
            // Get the stride (the amount of bytes per index)
            int stride = bitLength / 8;
            // Get the initial byte index
            int calculatedIndex = index * stride;
            int value = 0;
            for (int i = 0; i < stride; i++) {
                // Shift the value by a byte
                value <<= 8;
                // Or the next byte
                value |= arr[calculatedIndex+i];
            }
            // Return the value
            return value;
        }
        // Bit length is 8
        return arr[index];
    }
    
    private static void SetValue(byte[] arr, int bitLength, int index, int value) {
        // Empty palette can have only one possible value
        if (bitLength <= 1)
            return;
        // Bit length is less than a byte
        if (bitLength < 8) {
            // Calculate the bit mask
            int mask = (1 << bitLength) - 1;
            // Get the number of indices per byte
            int countPerByte = 8 / bitLength;
            // Calculate the byte index and offset
            (int calculatedIndex, int offset) = int.DivRem(index, countPerByte);
            // Get the byte
            int old = arr[calculatedIndex];
            // Get the amount to shift the mask by
            offset *= bitLength;
            // Mask the value
            old &= ~(mask << offset);
            // Or it with the value
            arr[calculatedIndex] = (byte)(old | ((value & mask) << offset));
            return;
        }
        // Bit length is more than a byte
        if (bitLength > 8) {
            // Get the stride (the amount of bytes per index)
            int stride = bitLength / 8;
            // Get the initial byte index
            int calculatedIndex = index * stride;
            for (int i = 0; i < stride; i++) {
                // Get the value for the index
                byte indexValue = (byte)((value >> (8*(stride-i-1))) & 0xFF);
                // Set the value
                arr[calculatedIndex+i] = indexValue;
            }
            return;
        }
        // Bit length is 8
        arr[index] = (byte)(value & 0xFF);
    }

    public void AddPaletteItem(TPaletteItem item) {
        // Don't add if it already exists
        if (PaletteIndeces.ContainsKey(item))
            return;
        // Add the item
        Palette.Add(item);
        PaletteIndeces[item] = Palette.Count - 1;
        // Upgrade the backing array if needed
        TryUpgradeBackingArray();
    }

    private void TryUpgradeBackingArray() {
        // Make sure new bit length is greater than current
        int newBitLength = CalculateBitLength(Palette.Count);
        if (newBitLength <= bitLength)
            return;
        // Create a new array with the right size
        int backingSize = (newBitLength * Size).CeilDiv(8);
        byte[] newArray = new byte[backingSize];
        // Copy over the data
        for (int i = 0; i < Size; i++)
            SetValue(newArray, newBitLength, i, GetValue(backingArray, bitLength, i));
        // Write the new bit length and new backing array
        bitLength = newBitLength;
        backingArray = newArray;
    }

    private void RemoveUnusedIndices() {
        // Fill a bit vector with all used palettes
        // There's probably a way to memoize this, but it's not important right now.
        var vector = new BitVector(Palette.Count);
        for (int i = 0; i < Size; i++)
            vector.Set(GetValue(backingArray, bitLength, i));
        
        int count = 0;
        int[] values = new int[vector.amountSet];
        // Iterate through the unset values
        foreach (int idx in vector.UnsetIndices()) {
            // Add it to an array of removed indices
            values[count] = idx;
            // Remove it from the list
            int offsetIndex = idx - count;
            var item = Palette[offsetIndex];
            Palette.RemoveAt(offsetIndex);
            PaletteIndeces.Remove(item);
            count++;
        }
        // Remove all the removed indices from the backing array
        RemovePaletteItems(values);

        // TODO: Downgrade backing array if bitLength changed.
    }

    private void RemovePaletteItems(params int[] indices) {
        // Iterate through all items in the backing array
        for (int i = 0; i < Size; i++) {
            int value = GetValue(backingArray, bitLength, i);
            int changedValue = value;
            // Decrement the changed value for all indices less than the initial value
            foreach (int index in indices)
                if (value >= index)
                    changedValue--;
            // Write it back to the backing array if it did change
            if (changedValue != value)
                SetValue(backingArray, bitLength, i, changedValue);
        }
    }
}
