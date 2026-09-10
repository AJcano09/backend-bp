using AccountService.Domain.Entities;
using AccountService.Domain.Exceptions;
using AccountService.Domain.ValueObjects;

namespace AccountService.Domain.Tests;

public class CuentaTests
{
    private static Cuenta CuentaConSaldo(decimal saldoInicial = 540, TipoCuenta tipo = TipoCuenta.Ahorros)
        => new(496825, tipo, saldoInicial, clienteId: 2);

    [Fact]
    public void SaldoDisponible_ConRetirosPrevios_RestaCadaRetiro()
    {
        var cuenta = CuentaConSaldo(saldoInicial: 540);

        // Regression: SaldoDisponible used to SUM the magnitudes of every
        // movement, so a second withdrawal was validated against an inflated
        // balance (540 + 540 = 1080) and never rejected despite the real
        // balance being zero after the first withdrawal.
        cuenta.RegistrarMovimiento(TipoMovimiento.Retiro, 540);   // leaves 0

        Assert.Throws<SaldoInsuficienteException>(() => cuenta.RegistrarMovimiento(TipoMovimiento.Retiro, 540));
        Assert.Equal(0m, cuenta.SaldoDisponible);
        Assert.Single(cuenta.Movimientos);
    }

    [Fact]
    public void Retiro_MayorAlSaldoDisponible_NoRegistraNada()
    {
        var cuenta = CuentaConSaldo(saldoInicial: 540);

        Assert.Throws<SaldoInsuficienteException>(() => cuenta.RegistrarMovimiento(TipoMovimiento.Retiro, 541));
        Assert.Empty(cuenta.Movimientos);
        Assert.Equal(540m, cuenta.SaldoDisponible);
    }

    [Fact]
    public void Retiro_ExactoAlSaldo_DejaSaldoEnCero()
    {
        var cuenta = CuentaConSaldo(saldoInicial: 540);
        var movimiento = cuenta.RegistrarMovimiento(TipoMovimiento.Retiro, 540);

        Assert.Equal(TipoMovimiento.Retiro, movimiento.TipoMovimiento);
        Assert.Equal(0m, movimiento.Saldo);
        Assert.Equal(0m, cuenta.SaldoDisponible);
    }

    [Fact]
    public void Deposito_SumaAlSaldoDisponible()
    {
        var cuenta = CuentaConSaldo(saldoInicial: 540);

        cuenta.RegistrarMovimiento(TipoMovimiento.Deposito, 600);

        Assert.Equal(1140m, cuenta.SaldoDisponible);
    }

    [Fact]
    public void SaldoDisponible_ConMovimientosMixtos_UsaElSignoDeCadaTipo()
    {
        var cuenta = CuentaConSaldo(saldoInicial: 2000);

        cuenta.RegistrarMovimiento(TipoMovimiento.Retiro, 575);    // 1425
        cuenta.RegistrarMovimiento(TipoMovimiento.Deposito, 600);  // 2025
        cuenta.RegistrarMovimiento(TipoMovimiento.Retiro, 540);    // 1485

        Assert.Equal(1485m, cuenta.SaldoDisponible);
    }
}