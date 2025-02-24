# Elliptic Curve Implementation

A C# implementation of elliptic curve arithmetic operations over finite fields, primarily focused on the mathematical fundamentals that underpin elliptic curve cryptography (ECC).

## Overview

This project provides a generic implementation of elliptic curves using the Weierstrass form: y² = x³ + ax + b over a finite field of prime characteristic p. It includes core operations like:

- Point addition
- Point doubling
- Scalar multiplication
- Point validation
- Point negation

## Usage Example

```csharp
// Create an elliptic curve y² = x³ + 2x + 3 (mod 17)
var curve = new EllipticCurve(
    a: 2,
    b: 3, 
    prime: 17,
    basePoint: new Point<EllipticCurve>(3, 6),
    order: 19
);

// Verify if a point lies on the curve
var point = new Point<EllipticCurve>(3, 6);
bool isValid = curve.IsOnCurve(point); // true

// Perform point addition
var sum = EllipticCurve.Add(curve, point1, point2);

// Scalar multiplication 
var result = EllipticCurve.ScalarMultiply(curve, point, k);
```

## Features

- **Generic Implementation**: Supports different curve types through the `IEllipticCurve` interface
- **Core Operations**:
    - Point addition and doubling
    - Scalar multiplication using double-and-add algorithm
    - Point validation
    - Point negation
- **Mathematical Utilities**:
    - Modular arithmetic operations
    - Extended Euclidean algorithm for modular inverse
- **Test Coverage**: Comprehensive test suite verifying curve operations

## Mathematical Background

The implementation uses the standard formulas for elliptic curve arithmetic:

- **Point Addition**: When P₁ = (x₁, y₁) ≠ ±P₂ = (x₂, y₂):
    - λ = (y₂ - y₁)/(x₂ - x₁)
    - x₃ = λ² - x₁ - x₂
    - y₃ = λ(x₁ - x₃) - y₁

- **Point Doubling**: When P = (x, y) and P is doubled:
    - λ = (3x² + a)/(2y)
    - x₃ = λ² - 2x
    - y₃ = λ(x - x₃) - y

All operations are performed modulo the prime characteristic p.

## Security Note

This implementation is primarily for educational purposes and demonstrates the mathematical concepts behind ECC. For production cryptographic purposes, use established cryptographic libraries.

## License

MIT
