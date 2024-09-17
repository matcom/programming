using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WEBOO.Programacion
{
    public class ArbolBinario<T>
    {

        public ArbolBinario(
            T valor,
            ArbolBinario<T> hijoIzquierdo,
            ArbolBinario<T> hijoDerecho)
        {
            this.Valor = valor;
            this.HijoIzquierdo = hijoIzquierdo;
            this.HijoDerecho = hijoDerecho;
        }

        public ArbolBinario(T valor) : this(valor, null, null) { }

        public T Valor { get; private set; }

        public ArbolBinario<T> HijoIzquierdo { get; protected set; }

        public ArbolBinario<T> HijoDerecho { get; protected set; }

        public IEnumerable<T> EntreOrden()
        {
            if (this.HijoIzquierdo != null)
                foreach (var x in this.HijoIzquierdo.EntreOrden())
                    yield return x;

            yield return this.Valor;

            if (this.HijoDerecho != null)
                foreach (var x in this.HijoDerecho.EntreOrden())
                    yield return x;
        }

        public IEnumerable<T> PostOrden()
        {
            if (this.HijoIzquierdo != null)
                foreach (var x in this.HijoIzquierdo.PostOrden())
                    yield return x;

            if (this.HijoDerecho != null)
                foreach (var x in this.HijoDerecho.PostOrden())
                    yield return x;

            yield return this.Valor;
        }

        public virtual bool Contiene(T x)
        {
            if (this.Valor.Equals(x))
                return true;
            if (this.HijoIzquierdo != null && this.HijoIzquierdo.Contiene(x))
                return true;
            if (this.HijoDerecho != null && this.HijoDerecho.Contiene(x))
                return true;

            return false;
        }

        #region EJERCICIOS
        //Programar los recorridos PreOrden y PostOrden
        //Determinar si un árbol binario es espejo
        //Programar un método que reciba un árbol correspondiente a una expresión (con operadores y valores enteros) y evalue la expresion
        #endregion
    }

    public class ArbolBinarioOrdenado<T> : ArbolBinario<T> where T : IComparable<T>
    {
        public ArbolBinarioOrdenado(T valor,
            ArbolBinarioOrdenado<T> hijoIzquierdo,
            ArbolBinarioOrdenado<T> hijoDerecho) : base(valor, hijoIzquierdo, hijoDerecho)
        {
            if ((hijoIzquierdo != null && hijoIzquierdo.Max.CompareTo(valor) > 0) ||
                (hijoDerecho != null && hijoDerecho.Min.CompareTo(valor) < 0))
                throw new ArgumentException();
        }

        public ArbolBinarioOrdenado(T valor) : this(valor, null, null) { }

        public new ArbolBinarioOrdenado<T> HijoDerecho
        {
            get { return base.HijoDerecho as ArbolBinarioOrdenado<T>; }
            protected set { base.HijoDerecho = value; }
        }

        public new ArbolBinarioOrdenado<T> HijoIzquierdo
        {
            get { return base.HijoIzquierdo as ArbolBinarioOrdenado<T>; }
            protected set { base.HijoIzquierdo = value; }
        }

        public T Max
        {
            get
            {
                return HijoDerecho == null ? this.Valor : HijoDerecho.Max;
            }
        }

        public T Min
        {
            get
            {
                return HijoIzquierdo == null ? this.Valor : HijoIzquierdo.Min;
            }
        }

        public override bool Contiene(T x)
        {
            int comparacion = x.CompareTo(this.Valor);

            if (comparacion < 0)
                if (HijoIzquierdo != null)
                    return HijoIzquierdo.Contiene(x);
                else
                    return false;

            if (comparacion > 0)
                if (HijoDerecho != null)
                    return HijoDerecho.Contiene(x);
                else
                    return false;

            return true;
        }

        #region EJERCICIOS
        //Determinar si un árbol binario está ordenado
        //Dado un árbol binario devolver un árbol binario ordenado
        //Determinar si un árbol binario está balanceado
        //Crear un árbol binario balanceado y ordenado a partir de uno ordenado
        #endregion
    }
}