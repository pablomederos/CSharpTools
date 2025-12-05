# Token Legend Mapping

This document defines the semantic token legend that **must be synchronized** between the C# backend (`TokenMapper.cs`) and the TypeScript frontend (`provider.ts` and `package.json`).

## Token Types

The following token types are defined with their corresponding indices:

| Index | Type | Description |
|-------|------|-------------|
| 0 | `class` | A class declaration |
| 1 | `interface` | An interface declaration |
| 2 | `enum` | An enumeration declaration |
| 3 | `struct` | A struct declaration |
| 4 | `method` | A method declaration |
| 5 | `property` | A property declaration |
| 6 | `variable` | A variable declaration |
| 7 | `parameter` | A parameter declaration |
| 8 | `namespace` | A namespace declaration |
| 9 | `type` | A generic type reference |

## Token Modifiers

Token modifiers are represented as a bitmask:

| Bit Position | Modifier | Description |
|--------------|----------|-------------|
| 0 (0x01) | `declaration` | Symbol is being declared |
| 1 (0x02) | `static` | Static member |
| 2 (0x04) | `readonly` | Readonly member |
| 3 (0x08) | `abstract` | Abstract member |

## Synchronization Checklist

When adding a new token type or modifier:

- [ ] Update `TokenMapper.cs` - `TokenTypes` array
- [ ] Update `TokenMapper.cs` - `GetTokenTypeIndex()` method
- [ ] Update `provider.ts` - `legend` definition
- [ ] Update `package.json` - `semanticTokenTypes` contribution
- [ ] Update this document

## Example

A `public static class Foo` would have:
- **TokenType**: 0 (class)
- **TokenModifiers**: 0x03 (declaration | static) = binary 0011
