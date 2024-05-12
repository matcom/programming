using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WEBOO.Programacion
{
    public class Cuenta
    {
        public string Titular { get; private set; }
        public float Saldo { get; protected set; }

        public Cuenta(string titular, float saldoInicial)
        {
            Titular = titular;
            if (saldoInicial < 0)
                throw new Exception("Hay que abrir una cuenta con saldo mayor que 0");
            Saldo = saldoInicial;
        }

        public virtual void Deposita(float cantidad)
        {
            if (cantidad <= 0)
                throw new Exception("Cantidad a depositar debe ser mayor que cero");
            Saldo += cantidad;
        }

        public virtual void Extrae(float cantidad)
        {
            if (cantidad <= 0)
                throw new Exception("Cantidad a extraer debe ser mayor que cero");
            else if (Saldo - cantidad < 0)
                throw new Exception("No hay saldo para extraer");
            Saldo -= cantidad;
        }


    }

    #region CUENTA CON TRANSFERENCIA USANDO HERENCIA
    public class CuentaTransferencia : Cuenta
    {

        public CuentaTransferencia(string titular, float saldoInicial) : base(titular, saldoInicial)
        { }

        public virtual void Transfiere(float cantidad, Cuenta otraCuenta)
        {
            if (cantidad <= 0)
                throw new Exception("Cantidad a transferir debe ser mayor que cero");
            else if (Saldo >= cantidad)
            {
                otraCuenta.Deposita(cantidad);
                Extrae(cantidad);
            }
            else
                throw new Exception("No hay saldo suficiente para hacer el traspaso");
        }

    }
    #endregion

    #region CREDITO CON HERENCIA
    //Para probar principio de sustitución añadir override en la redefinición de Extrae
    public class CuentaCredito : Cuenta
    {
        public float Interes { get; private set; }

        public CuentaCredito(string titular, float saldoInicial, float tasaInteres = 5) : base(titular, saldoInicial)
        {
            if (tasaInteres > 0 && tasaInteres < 100)
                Interes = tasaInteres;
            else throw new Exception("Tasa interés incorrecta");
        }

        public override void Extrae(float cantidad) // probar con `new` para ver que no funciona el ejemplo.
        {
            if (cantidad <= 0)
                throw new Exception("Cantidad a extraer debe ser mayor que cero");
            else if (Saldo >= cantidad)
                Saldo -= cantidad;
            else // No hay saldo suficiente se va extraer a crédito con X% de interés
                Saldo -= (cantidad + (cantidad * Interes / 100));
        }
    }
    #endregion

}