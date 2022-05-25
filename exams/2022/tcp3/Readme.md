# TuEnvío 2.0

Usted ha sido contratado para la remodelación y actualización de la plataforma TuEnvío.
En concreto, su trabajo consistirá en implementar la funcionalidad del
carrito de compras.

Como su código debe coexistir con la arquitectura de software existente,
usted debe implementar la `interface IShoppingCart`:

```cs
public interface IShoppingCart<TProduct> where TProduct : IProduct
{
    int Cost { get; }
    int Total { get; }

    void Add(TProduct product, int count);
    bool Remove(TProduct product);
    int Count(TProduct product);
    int Count(IFilter<TProduct> filter);
    void AddPromotion(IPromotion<TProduct> promotion);
}
```

Esta `interface` encapsula las responsabilidades de un carrito de compras,
que consiste en almacenar una colección de productos (`inteface IProduct`)
y calcular el costo total de la compra.

Adicionalmente, el carrito de compras puede tener promociones (`interface IPromotion`)
que permiten aplicar descuentos a diferentes productos por diferentes criterios.
También es posible consultar el estado del carrito de compras según diferentes filtros
(`interface IFilter`).

En su implementación, usted debe hacer uso de las interfaces `IProduct`, `IFilter`
e `IPromotion` de manera que su implementación de `IShoppingCart` sea compatible
con cualquier implementación de estas `interfaces` que los otros desarrolladores creen.

Para proveer su implementación de `IShoppingCart` a los demás clientes de su código,
usted debe implementar el método `GetShoppingCart` de la clase `TuEnvio`.
Este método recibe un parámetro `capacity` que representa la cantidad máxima de productos
diferentes que pueden ser adicionados al carrito. Se garantiza que nunca se adicionarán
una cantidad mayor de productos **diferentes** que este valor.

```cs
public static class TuEnvio
{
    public static IShoppingCart<TProduct> GetShoppingCart<TProduct>(int capacity)
        where TProduct : IProduct
    {
        // Borre aquí y devuelva una instancia de su implementación de IShoppingCart
        throw new NotImplementedException();
    }
}
```

A continuación se explica en más detalle las funcionalidades a implementar.

## Manejo de productos

La `interface IProduct` encapsula el concepto de un producto a ubicar en el carrito:

```cs
public interface IProduct
{
    int Price { get; }
    string Description { get; }
}
```

Todo producto tiene un precio base (propiedad `Price`) y una descripcion (propiedad `Description`).

El carrito de compras debe permitir adicionar una o más unidades de cualquier producto a partir
del método `Add`.

```cs
interface IShoppingCart<TProduct> where TProduct: IProduct
{
    // ...
    void Add(TProduct product, int count);
    // ...
}
```

Note que `IShoppingCart` es genérico en el tipo de producto.

A los efectos de su implementación, dos instancias de `IProduct` se consideran el mismo producto
si y solo si el método `Equals` de la instancia correspondiente devuelve `true`,
independientemente del valor de las propiedades `Price` o `Description`.

Teniendo esto en cuenta, es equivalente invocar `Add` dos veces con instancias del mismo
producto con diferentes valores para `count`, que invocar `Add` una sola vez con la suma de
ambos valores.

Además de adicionar, el carrito de compras permite eliminar todas las unidades de un producto
mediante el método `Remove`:

```cs
interface IShoppingCart<TProduct> where TProduct: IProduct
{
    // ...
    bool Remove(TProduct product);
    // ...
}
```

Este método devuelve `true` si el producto existía en el carrito, y posteriormente a su invocación
no debe quedar ninguna unidad del mismo producto, independientemente de cuántas veces se haya
invocado `Add` con el mismo producto.

En cualquier momento, la propiedad `Total` de `IShoppingCart` debe devolver la cantidad total
de productos diferentes en el carrito, independientemente de la cantidad de unidades de cada producto.

El carrito permite además saber la cantidad de unidades de un producto específico mediante el método
`Count`:

```cs
interface IShoppingCart<TProduct> where TProduct: IProduct
{
    // ...
    int Count(TProduct product);
    // ...
}
```

Este método debe devolver la cantidad total de unidades del producto correspondiente,
independientemente de cuántas veces se haya invocado `Add` con el mismo producto.

## Punto de entrada

```cs
namespace TuEnvio
{
    public static class Library
    {
        public static IShoppingCart<TProduct> GetShoppingCart<TProduct>(int capacity)
            where TProduct : IProduct
        {
            // Borre aquí y devuelva una instancia de su implementación
            // de IShoppingCart
            throw new NotImplementedException();
        }
    }
}
```