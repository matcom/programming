using System;
using System.Collections.Generic;

namespace MatCom.Exam
{
    public class Exam
    {
        public static IInventory GetInventory() => new MyInventory(new MyCategory(""));

        // Borre esta excepción y ponga su nombre como string, e.j.
        // Nombre => "Fulano Pérez Pérez";
        public static string Nombre => "Tony Cadahia Podeva";

        // Borre esta excepción y ponga su grupo como string, e.j.
        // Grupo => "C2XX";
        public static string Grupo => "C312";
    }

    public class MyInventory : IInventory
    {   
        /// <summary>
        /// Representa la categoria raiz en una definicion de MyInventory
        /// </summary>
        public ICategory Root { get; private set; }

        /// <summary>
        /// Constructor de la definicion MyInventory
        /// </summary>
        /// <param name="root"></param>
        public MyInventory(ICategory root)
        {
            //Si se para arriba de los parametros o metodos aparece una breve descripcion
            
            Root = root;
        }

        /// <summary>
        /// Metodo FindAll de la interfaz IInventory
        /// </summary>
        /// <param name="filter">Representa el metodo que determina si se va a escoger ese producto o no</param>
        /// <returns>Retorna un IEnumerable de los productos que cumplen con filter</returns>
        public IEnumerable<IProduct> FindAll(Filter<IProduct> filter) => FindAll(Root, filter);

        /// <summary>
        /// Una sobrecarga del metodo FindAll de la interfaz IInventory
        /// </summary>
        /// <param name="actual">Representa la categoria actual</param>
        /// <param name="filter">Representa el metodo que determina si se va a escoger ese producto o no</param>
        /// <returns>Retorna un IEnumerable de los productos que cumplen con filter</returns>
        public IEnumerable<IProduct> FindAll(ICategory actual, Filter<IProduct> filter)
        {
            //Si se para arriba de los parametros o metodos aparece una breve descripcion

            //Se hace un recorrido por todas las categorias dentro de la categoria actual
            foreach (var item1 in actual.Subcategories)
            {
                //Se crea el enumerable de la subcategoria y se devuelven todos los elementos de esta
                foreach (var item2 in FindAll(item1, filter))
                {
                    yield return item2;
                }
            }

            //Se hace un recorrido por todos los productos dentro de la categoria actual
            foreach (var item in actual.Products)
            {
                //Si no cumple con la propiedad se ignora el item, si no entonces se devuelve
                if (!filter(item)) continue;
                yield return item;
            }
        }

        /// <summary>
        /// Metodo GetCategory de la interfaz IInventory
        /// </summary>
        /// <param name="categories">Representa el camino a seguir en el arbol de categorias</param>
        /// <returns>Retorna la ultima categoria de categories si existe</returns>
        public ICategory GetCategory(params string[] categories) => GetCategory(0, Root, categories);

        /// <summary>
        /// Una sobrecarga del metodo GetCategory de la interfaz IInventory
        /// </summary>
        /// <param name="i">Representa el indice de la categoria a analizar en categories</param>
        /// <param name="actual">Representa la categoria actual</param>
        /// <param name="categories">Representa el camino a seguir en el arbol de categorias</param>
        /// <returns>Retorna la ultima categoria de categories si existe</returns>
        /// <exception cref="ArgumentException">En caso de que no exista alguna categoria en el camino se lanzara una excepcion</exception>
        public ICategory GetCategory(int i, ICategory actual, params string[] categories)
        {
            //Si se para arriba de los parametros o metodos aparece una breve descripcion

            //Este es el caso base, si ya no quedan elementos en el array y no se ha 
            //lanzado error entonces la categoria actual es correcta
            if (i == categories.Length) return actual;

            //Se hace un recorrido por todas las categorias dentro de la categoria actual
            foreach (var item in actual.Subcategories)
            {
                //Si no coincide el nombre se ignora el item, si no entonces retorna el valor recursivo asumiendo que existe
                if (item.Name != categories[i]) continue;
                return GetCategory(++i, item, categories);
            }
            
            //Si llega a esta linea es porque no existe ese nombre en la lista de subcategorias actuales, luego se lanza una excepcion
            throw new ArgumentException("esta categoria no existe");
        }

        /// <summary>
        /// Metodo GetProduct de la interfaz IInventory
        /// </summary>
        /// <param name="name">Representa el nombre del producto a devolver</param>
        /// <param name="categories">Representa el camino a seguir en el arbol de categorias</param>
        /// <returns>Retorna el producto siguiendo el camino de categories si existe</returns>
        /// <exception cref="ArgumentException">Suponiendo que exista un camino y no exista el producto se lanza excepcion</exception>
        public IProduct GetProduct(string name, params string[] categories)
        {
            //Si se para arriba de los parametros o metodos aparece una breve descripcion

            //Se recorren los productos de la categoria correspondiente al camino categories
            foreach (var item in GetCategory(categories).Products)
            {
                //Si no coincide el nombre se ignora el item, si no entonces retorna el valor
                if (item.Name != name) continue;
                return item;
            }

            //Si llega a esta linea es porque no existe ese nombre en la lista de productos actuales, asumiendo que existia un camino en categories
            throw new ArgumentException("este producto no existe");
        }
    }

    public class MyCategory : ICategory
    {
        /// <summary>
        /// Nombre de la categoria
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Categoria superior en la jerarquia de categorias-productos
        /// </summary>
        public ICategory Parent { get; private set; }

        /// <summary>
        /// Representa la lista de categorias dentro de la definicion de MyCategory
        /// </summary>
        private List<ICategory> subCategories;

        /// <summary>
        /// Representa la lista de productos dentro de la definicion de MyCategory
        /// </summary>
        private List<IProduct> products;

        /// <summary>
        /// Constructor de la definicion MyCategory
        /// </summary>
        /// <param name="name">Nombre de la categoria</param>
        /// <param name="parent">Categoria superior en la jerarquia de categorias, si no se define se toma como ella misma</param>
        public MyCategory(string name, ICategory parent)
        {
            //Si se para arriba de los parametros o metodos aparece una breve descripcion

            Name = name;
            Parent = parent;
            subCategories = new List<ICategory>();
            products = new List<IProduct>();
        }

        public MyCategory(string name)
        {
            //Si se para arriba de los parametros o metodos aparece una breve descripcion

            Name = name;
            Parent = this;
            subCategories = new List<ICategory>();
            products = new List<IProduct>();
        }

        /// <summary>
        /// Representa la lista de subcategorias
        /// </summary>
        public IEnumerable<ICategory> Subcategories => subCategories;

        /// <summary>
        /// Representa la lista de productos excluyendo los que son menores que 0
        /// </summary>
        public IEnumerable<IProduct> Products => products.FindAll(x => x.Count > 0);

        /// <summary>
        /// Metodo CreateSubcategory de la interfaz IInventory
        /// </summary>
        /// <param name="name">Nombre de la categoria</param>
        /// <returns>Retorna la categoria creada</returns>
        public ICategory CreateSubcategory(string name)
        {
            //Si se para arriba de los parametros o metodos aparece una breve descripcion

            //Si existe ya la categoria entonces lanza excepcion
            if (subCategories.Exists(x => x.Name == name)) throw new ArgumentException("Ya esta categoria existe");
            MyCategory newCategory = new MyCategory(name, this);
            subCategories.Add(newCategory);
            return newCategory;
        }

        /// <summary>
        /// Metodo UpdateProduct de la interfaz IInventory
        /// </summary>
        /// <param name="product">Nombre del porducto</param>
        /// <param name="count">Cantidad de instancias del producto a crear</param>
        public void UpdateProduct(string product, int count)
        {
            //Si se para arriba de los parametros o metodos aparece una breve descripcion

            //Se busca el producto si existe
            MyProduct? actualproduct = (MyProduct?)products.Find(x => x.Name.Equals(product));
            //si existe se actualiza, si no se añade a la lista de productos
            if (actualproduct != null) actualproduct.Count += count;
            else products.Add(new MyProduct(product, count, this));
        }
    }
    
    public class MyProduct : IProduct
    {
        /// <summary>
        /// Representa la cantidad de instancias del producto dentro de la definicion de MyProduct
        /// </summary>
        private int count;

        /// <summary>
        /// Representa la cantidad de instancias del producto
        /// </summary>
        public int Count 
        { 
            get 
            { 
                return count; 
            }
            set
            {
                //Si el valor que se le asigna es negativo se lanza una excepcion
                if (value < 0) throw new ArgumentException("el producto no puede tener un valor negativo");
                count = value;
            }
        }

        /// <summary>
        /// Categoria superior en la jerarquia de categorias-producto
        /// </summary>
        public ICategory Parent { get; private set; }

        /// <summary>
        /// Representa el nombre del producto
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor de la definicion MyProduct
        /// </summary>
        /// <param name="name">nombre del producto</param>
        /// <param name="count">cantidad de instancias del producto</param>
        /// <param name="parent">Categoria superior en la jerarquia de categorias-producto</param>
        public MyProduct(string name, int count, ICategory parent)
        {
            //Si se para arriba de los parametros o metodos aparece una breve descripcion

            Name = name;
            Count = count;
            Parent = parent;
        }
    }
}