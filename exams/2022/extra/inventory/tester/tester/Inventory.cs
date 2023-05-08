using System.Collections.Generic;

namespace MatCom.Exam;

public interface IInventory
{
    // Categoría raíz
    ICategory Root { get; }

    // Navegar por las categorías y productos
    ICategory GetCategory(params string[] categories);
    IProduct GetProduct(string name, params string[] categories);

    // Buscar los productos que cumplen con una condición
    IEnumerable<IProduct> FindAll(Filter<IProduct> filter);
}

public interface ICategory
{
    string Name { get; }

    // Crear subcategorías
    ICategory CreateSubcategory(string name);

    // Crear o actualizar productos
    void UpdateProduct(string product, int count);

    // Enumerar todas las subcategorías (en este nivel)
    IEnumerable<ICategory> Subcategories { get; }

    // Enumerar todos los productos (en este nivel)
    IEnumerable<IProduct> Products { get; }

    // Categoría padre
    ICategory Parent { get; }
}

public interface IProduct
{
    string Name { get; }
    int Count { get; }
    ICategory Parent { get; }
}

public delegate bool Filter<T>(T item);
