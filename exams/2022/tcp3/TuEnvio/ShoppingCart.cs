namespace TuEnvio
{
    public static class Library
    {
        public static IShoppingCart<TProduct> GetShoppingCart<TProduct>()
            where TProduct : IProduct
        {
            // Borre aquí y devuelva una instancia de su implementación
            // de IShoppingCart
            throw new NotImplementedException();
        }
    }
}