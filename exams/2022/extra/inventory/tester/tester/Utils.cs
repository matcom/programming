
using Xunit;
using System.Linq;
using MatCom.Exam;
using System.Collections.Generic;

namespace MatCom.Tester;

public enum TestType
{
    RootCase,
    CreateSubcategory,
    UpdateProduct,
    Subcategories,
    CategoryParent,
    ProductParent,
    GetCategory,
    GetProduct,
    FindAll,
    CreateSubcategoryException,
    UpdateProductException,
    GetProductException,
    GetCategoryException,
    CombinedTest1,
    CombinedTest2,
    CombinedTest3
}

public static class Utils
{
    public static ICategory CreateSubcategory(ICategory parent, string name)
    {
        var child = parent.CreateSubcategory(name);

        Assert.Equal(child.Name, name); 
        Assert.False(child.Products.GetEnumerator().MoveNext());
        Assert.False(child.Subcategories.GetEnumerator().MoveNext());
        
        return child;
    }

    public static void UpdateProduct(ICategory category, string name, int count)
    {
        category.UpdateProduct(name, count);

        Assert.Contains( category.Products, 
            item => item.Name == name && item.Count == count);
    }

    public static IProduct GetProduct(IInventory inventory, string name, 
        int count, params string[] categories)
    {
        var product = inventory.GetProduct(name, categories);

        Assert.True(name == product.Name);
        Assert.True(count == product.Count);
        Assert.True(product.Parent.Name == categories.Last());

        return product;
    }

    public static IList<ICategory> Path(IProduct product)
    {
        var path = new List<ICategory>();
        var current = product.Parent;
        ICategory? previous = null;

        while (current != previous)
        {
            path.Add(current);
            previous = current;
            current = current.Parent;
        }
        path.Reverse();

        return path;
    }

    public static bool IsDescendantOf(IProduct product, ICategory ancestor) 
        => Path(product).Contains(ancestor);

    public static IInventory CreateInventory(int seed, IList<TestType> tests)
    {
        var inventory = Exam.Exam.GetInventory();
        var rootCategory = inventory.Root;
        
        #region Home appliances

        var homeAppliances = CreateSubcategory(rootCategory, $"HomeAppliances_{seed + 1}");

        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(rootCategory, homeAppliances.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
           Assert.Contains(rootCategory.Subcategories, 
                c => c.Name == $"HomeAppliances_{seed + 1}" && c.Parent == rootCategory);
        }
        
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(homeAppliances, 
                inventory.GetCategory($"HomeAppliances_{seed + 1}"));
        }

        #region Televisions

        var televisions = CreateSubcategory(homeAppliances, $"Televisions_{seed + 2}");
                           
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(homeAppliances, televisions.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(homeAppliances.Subcategories, 
                c => c.Name == $"Televisions_{seed + 2}" && c.Parent == homeAppliances);
        }
        
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(televisions, 
                inventory.GetCategory($"HomeAppliances_{seed + 1}", $"Televisions_{seed + 2}"));
        }

        #region Curved televisions

        var curvedTvs = CreateSubcategory(televisions, $"CurvedTvs_{seed + 3}");
                                    
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(televisions, curvedTvs.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(televisions.Subcategories, 
                c => c.Name == $"CurvedTvs_{seed + 3}" && c.Parent == televisions);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(curvedTvs, 
                inventory.GetCategory($"HomeAppliances_{seed + 1}", $"Televisions_{seed + 2}", $"CurvedTvs_{seed + 3}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(curvedTvs, $"Samsung_{seed + 4}", 4);
            UpdateProduct(curvedTvs, $"LG_{seed + 4}", 2);
            UpdateProduct(curvedTvs, $"Sony_{seed + 4}", 3);
            UpdateProduct(curvedTvs, $"Panasonic_{seed + 4}", 1);
            UpdateProduct(curvedTvs, $"Toshiba_{seed + 4}", 1);
            UpdateProduct(curvedTvs, $"Philips_{seed + 4}", 1);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(curvedTvs.Products, 
                    p => Assert.Equal(curvedTvs, p.Parent));
        }

        #endregion

        #region Flat televisions

        var flatTvs = CreateSubcategory(televisions, $"FlatTvs_{seed + 3}");
                                             
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(televisions, flatTvs.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(televisions.Subcategories, 
                c => c.Name == $"FlatTvs_{seed + 3}" && c.Parent == televisions);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(flatTvs, 
                inventory.GetCategory($"HomeAppliances_{seed + 1}", $"Televisions_{seed + 2}", $"FlatTvs_{seed + 3}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(flatTvs, $"Samsung_{seed + 4}", 4);
            UpdateProduct(flatTvs, $"LG_{seed + 4}", 2);
            UpdateProduct(flatTvs, $"Sony_{seed + 4}", 3);
            UpdateProduct(flatTvs, $"Panasonic_{seed + 4}", 1);
            UpdateProduct(flatTvs, $"Toshiba_{seed + 4}", 1);
            UpdateProduct(flatTvs, $"Philips_{seed + 4}", 1);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(flatTvs.Products, 
                    p => Assert.Equal(flatTvs, p.Parent));
        }

        #endregion

        #endregion

        #region Refrigerators

        var refrigerators = CreateSubcategory(homeAppliances, $"Refrigerators_{seed + 2}");
                                                      
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(homeAppliances, refrigerators.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(homeAppliances.Subcategories, 
                c => c.Name == $"Refrigerators_{seed + 2}" && c.Parent == homeAppliances);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(refrigerators, 
                inventory.GetCategory($"HomeAppliances_{seed + 1}", $"Refrigerators_{seed + 2}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(refrigerators, $"Philips_{seed + 3}", 12);
            UpdateProduct(refrigerators, $"LG_{seed + 3}", 24);
            UpdateProduct(refrigerators, $"Samsung_{seed + 3}", 12);
            UpdateProduct(refrigerators, $"Sony_{seed + 3}", 14);
            UpdateProduct(refrigerators, $"Daewoo_{seed + 3}", 5);
            UpdateProduct(refrigerators, $"Panasonic_{seed + 3}", 20);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(refrigerators.Products, 
                   p => Assert.Equal(refrigerators, p.Parent));
        }

        #endregion

        #region Washing machines

        var washingMachines = CreateSubcategory(homeAppliances, $"WashingMachines_{seed + 2}");
                                                               
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(homeAppliances, washingMachines.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(homeAppliances.Subcategories, 
                c => c.Name == $"WashingMachines_{seed + 2}" && c.Parent == homeAppliances);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(washingMachines, 
                inventory.GetCategory($"HomeAppliances_{seed + 1}", $"WashingMachines_{seed + 2}"));
        }

        #endregion

        #region Fans

        var fans = CreateSubcategory(homeAppliances, $"Fans_{seed + 2}");
                                                                        
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(homeAppliances, fans.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(homeAppliances.Subcategories, 
                c => c.Name == $"Fans_{seed + 2}" && c.Parent == homeAppliances);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(fans, 
                inventory.GetCategory($"HomeAppliances_{seed + 1}", $"Fans_{seed + 2}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(fans, $"Philips_{seed + 3}", 12);
            UpdateProduct(fans, $"LG_{seed + 3}", 24);
            UpdateProduct(fans, $"Samsung_{seed + 3}", 12);
            UpdateProduct(fans, $"Sony_{seed + 3}", 14);
            UpdateProduct(fans, $"Daewoo_{seed + 3}", 5);
            UpdateProduct(fans, $"Panasonic_{seed + 3}", 20);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(fans.Products, 
                    p => Assert.Equal(fans, p.Parent));
        }

        #endregion

        #region Microwaves

        var microwaves = CreateSubcategory(homeAppliances, $"Microwaves_{seed + 2}");
                                                                                 
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(homeAppliances, microwaves.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(homeAppliances.Subcategories, 
                c => c.Name == $"Microwaves_{seed + 2}" && c.Parent == homeAppliances);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(microwaves, 
                inventory.GetCategory($"HomeAppliances_{seed + 1}", $"Microwaves_{seed + 2}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(microwaves, $"Samsung_{seed + 3}", 4);
            UpdateProduct(microwaves, $"Panasonic_{seed + 3}", 4);
            UpdateProduct(microwaves, $"LG_{seed + 3}", 4);
            UpdateProduct(microwaves, $"Midea_{seed + 3}", 4);
            UpdateProduct(microwaves, $"Daewoo_{seed + 3}", 4);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(microwaves.Products, 
                    p => Assert.Equal(microwaves, p.Parent));
        }

        #endregion

        #region Splits 

        var splits = CreateSubcategory(homeAppliances, $"Splits_{seed + 2}");
                                                                                          
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(homeAppliances, splits.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(homeAppliances.Subcategories, 
                c => c.Name == $"Splits_{seed + 2}" && c.Parent == homeAppliances);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(splits, 
                inventory.GetCategory($"HomeAppliances_{seed + 1}",  $"Splits_{seed + 2}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(splits, $"Carrier_{seed + 3}", 4);
            UpdateProduct(splits, $"Royal_{seed + 3}", 4);
            UpdateProduct(splits, $"Samsung_{seed + 3}", 4);
            UpdateProduct(splits, $"Panasonic_{seed + 3}", 4);
            UpdateProduct(splits, $"LG_{seed + 3}", 4);
            UpdateProduct(splits, $"Midea_{seed + 3}", 4);
            UpdateProduct(splits, $"Daewoo_{seed + 3}", 4);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(splits.Products, 
                    p => Assert.Equal(splits, p.Parent));
        }

        #endregion

        #endregion

        #region Foods products

        var foods = CreateSubcategory(rootCategory, $"Foods_{seed + 1}");
                                                                                                   
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(rootCategory, foods.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(rootCategory.Subcategories, 
                c => c.Name == $"Foods_{seed + 1}" && c.Parent == rootCategory);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(foods, 
                inventory.GetCategory($"Foods_{seed + 1}"));
        }

        #region Dairy products

        var dairy = CreateSubcategory(foods, $"Dairy_{seed + 2}");
                                                                                                            
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(foods, dairy.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(foods.Subcategories, 
                c => c.Name == $"Dairy_{seed + 2}" && c.Parent == foods);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(dairy, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Dairy_{seed + 2}"));
        }

        var milk = CreateSubcategory(dairy, $"Milk_{seed + 3}");
                                                                                                                     
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(dairy, milk.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(dairy.Subcategories, 
                c => c.Name == $"Milk_{seed + 3}" && c.Parent == dairy);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(milk, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Dairy_{seed + 2}", $"Milk_{seed + 3}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(milk, $"1Kg_bag_of_powdered_milk_{seed + 4}", 10);
            UpdateProduct(milk, $"1Lt_milk_bottle_{seed + 4}", 10);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(milk.Products, 
                    p => Assert.Equal(milk, p.Parent));
        }

        var yogurt = CreateSubcategory(dairy, $"Yogurt_{seed + 3}");
                                                                                                                              
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(dairy, yogurt.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(dairy.Subcategories, 
                c => c.Name == $"Yogurt_{seed + 3}" && c.Parent == dairy);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(yogurt, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Dairy_{seed + 2}", $"Yogurt_{seed + 3}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(yogurt, $"cup_of_natural_yogurt_{seed + 4}", 220);
            UpdateProduct(yogurt, $"5Lt_yogurt_tanks_{seed + 4}", 50);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(yogurt.Products, 
                    p => Assert.Equal(yogurt, p.Parent));
        }

        var cheese = CreateSubcategory(dairy, $"Cheese_{seed + 3}");
                                                                                                                                       
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(dairy, cheese.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(dairy.Subcategories, 
                c => c.Name == $"Cheese_{seed + 3}" && c.Parent == dairy);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(cheese, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Dairy_{seed + 2}", $"Cheese_{seed + 3}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(cheese, $"1Kg_of_white_cheese_bars_{seed + 4}", 70);
            UpdateProduct(cheese, $"1Kg_of_gouda_cheese_bars_{seed + 4}", 55);
            UpdateProduct(cheese, $"1Lb_bag_of_grated_parmesan_cheese_{seed + 4}", 40);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(cheese.Products, 
                    p => Assert.Equal(cheese, p.Parent));
        }

        #endregion

        #region Meat products

        var meat = CreateSubcategory(foods, $"Meat_{seed + 2}");
        
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(foods, meat.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(foods.Subcategories, 
                c => c.Name == $"Meat_{seed + 2}" && c.Parent == foods);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(meat, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Meat_{seed + 2}"));
        }

        var chicken = CreateSubcategory(meat, $"Chicken_{seed + 3}");
        
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(meat, chicken.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(meat.Subcategories, 
                c => c.Name == $"Chicken_{seed + 3}" && c.Parent == meat);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(chicken, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Meat_{seed + 2}", $"Chicken_{seed + 3}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(chicken, $"1Kg_of_chicken_{seed + 4}", 100);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(chicken.Products, 
                    p => Assert.Equal(chicken, p.Parent));
        }

        var beef = CreateSubcategory(meat, $"Beef_{seed + 3}");
                 
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(meat, beef.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(meat.Subcategories, 
                c => c.Name == $"Beef_{seed + 3}" && c.Parent == meat);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(beef, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Meat_{seed + 2}", $"Beef_{seed + 3}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(beef, $"1Kg_of_beef_steak_{seed + 4}", 100);
            UpdateProduct(beef, $"1Kg_of_beef_ribs_{seed + 4}", 100);
            UpdateProduct(beef, $"1Kg_of_beef_sausage_{seed + 4}", 100);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(beef.Products, 
                    p => Assert.Equal(beef, p.Parent));
        }

        var pork = CreateSubcategory(meat, $"Pork_{seed + 3}");
                          
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(meat, pork.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(meat.Subcategories, 
                c => c.Name == $"Pork_{seed + 3}" && c.Parent == meat);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(pork, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Meat_{seed + 2}", $"Pork_{seed + 3}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(pork, $"1Kg_of_pork_chops_{seed + 4}", 100);
            UpdateProduct(pork, $"1Kg_of_pork_ribs_{seed + 4}", 100);
            UpdateProduct(pork, $"1Kg_of_pork_sausages_{seed + 4}", 100);
            UpdateProduct(pork, $"1Kg_of_pork_tenderloin_{seed + 4}", 100);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(pork.Products, 
                    p => Assert.Equal(pork, p.Parent));
        }

        var fish = CreateSubcategory(meat, $"Fish_{seed + 3}");
                                   
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(meat, fish.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(meat.Subcategories, 
                c => c.Name == $"Fish_{seed + 3}" && c.Parent == meat);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(fish, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Meat_{seed + 2}", $"Fish_{seed + 3}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(fish, $"1Kg_of_salmon_{seed + 4}", 83);
            UpdateProduct(fish, $"1Kg_of_tuna_{seed + 4}", 24);
            UpdateProduct(fish, $"1Kg_of_trout_{seed + 4}", 55);
            UpdateProduct(fish, $"1Kg_of_cod_{seed + 4}", 13);
            UpdateProduct(fish, $"1Kg_of_mackerel_{seed + 4}", 32);
            UpdateProduct(fish, $"1Kg_of_sardines_{seed + 4}", 58);
            UpdateProduct(fish, $"1Kg_of_anchovies_{seed + 4}", 69);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(fish.Products, 
                    p => Assert.Equal(fish, p.Parent));
        }

        #endregion

        #region Vegetables products

        var vegetables = CreateSubcategory(foods, $"Vegetables_{seed + 2}");
                                            
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(foods, vegetables.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(foods.Subcategories, 
                c => c.Name == $"Vegetables_{seed + 2}" && c.Parent == foods);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(vegetables, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Vegetables_{seed + 2}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(vegetables, $"1Kg_of_carrots_{seed + 3}", 83);
            UpdateProduct(vegetables, $"1Kg_of_celery_{seed + 3}", 13);
            UpdateProduct(vegetables, $"1Kg_of_cucumbers_{seed + 3}", 58);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(vegetables.Products, 
                    p => Assert.Equal(vegetables, p.Parent));
        }

        var onions = CreateSubcategory(vegetables, $"Onions_{seed + 3}");
                                                     
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(vegetables, onions.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(vegetables.Subcategories, 
                c => c.Name == $"Onions_{seed + 3}" && c.Parent == vegetables);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(onions, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Vegetables_{seed + 2}", $"Onions_{seed + 3}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(onions, $"1Kg_of_chives_{seed + 4}", 13);
            UpdateProduct(onions, $"1Kg_of_purple_onions_{seed + 4}", 50);
            UpdateProduct(onions, $"1Kg_of_white_onions_{seed + 4}", 50);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(onions.Products, 
                    p => Assert.Equal(onions, p.Parent));
        }

        #endregion

        #region Fruits products

        var fruits = CreateSubcategory(foods, $"Fruits_{seed + 2}");
                                                              
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(foods, fruits.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(foods.Subcategories, 
                c => c.Name == $"Fruits_{seed + 2}" && c.Parent == foods);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(fruits, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Fruits_{seed + 2}"));
        }

        var apples = CreateSubcategory(fruits, $"Apples_{seed + 3}");
                                                                               
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(fruits, apples.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(foods.Subcategories, 
                c => c.Name == $"Fruits_{seed + 2}" && c.Parent == foods);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(apples, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Fruits_{seed + 2}", $"Apples_{seed + 3}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(apples, $"1Kg_of_red_apples_{seed + 4}", 74);
            UpdateProduct(apples, $"1Kg_of_green_apples_{seed + 4}", 47);
            UpdateProduct(apples, $"1Kg_of_yellow_apples_{seed + 4}", 55);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(apples.Products, 
                    p => Assert.Equal(apples, p.Parent));
        }

        var oranges = CreateSubcategory(fruits, $"Oranges_{seed + 3}");
                                                                                                  
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(fruits, oranges.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(foods.Subcategories, 
                c => c.Name == $"Fruits_{seed + 2}" && c.Parent == foods);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(oranges, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Fruits_{seed + 2}", $"Oranges_{seed + 3}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(oranges, $"1Kg_of_mandarin_{seed + 4}", 14);
            UpdateProduct(oranges, $"1Kg_of_bitter_orange_{seed + 4}", 83);
            UpdateProduct(oranges, $"1Kg_of_blood_orange_{seed + 4}", 84);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(oranges.Products, 
                    p => Assert.Equal(oranges, p.Parent));
        }

        var grapes = CreateSubcategory(fruits, $"Grapes_{seed + 3}");
                                                                                                                       
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(fruits, grapes.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(foods.Subcategories, 
                c => c.Name == $"Fruits_{seed + 2}" && c.Parent == foods);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(grapes, 
                inventory.GetCategory($"Foods_{seed + 1}",  $"Fruits_{seed + 2}", $"Grapes_{seed + 3}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(grapes, $"1Kg_of_blackberries_{seed + 4}", 60);
            UpdateProduct(grapes, $"1Kg_of_blueberries_{seed + 4}", 65);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(grapes.Products, 
                    p => Assert.Equal(grapes, p.Parent));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(fruits, $"1Kg_of_bananas_{seed + 4}", 100);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(fruits.Products, 
                    p => Assert.Equal(fruits, p.Parent));
        }

        #endregion

        #region Grain products

        var grains = CreateSubcategory(foods, $"Grains_{seed + 2}");
                                                                                                                                      
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(foods, grains.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(foods.Subcategories, 
                c => c.Name == $"Grains_{seed + 2}" && c.Parent == foods);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(grains, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Grains_{seed + 2}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(grains, $"1Kg_of_barley_{seed + 3}", 55);
            UpdateProduct(grains, $"1Kg_of_bulgur_{seed + 3}", 74);
            UpdateProduct(grains, $"1Kg_of_farro_{seed + 3}", 40);
            UpdateProduct(grains, $"1Kg_of_freekeh_{seed + 3}", 83);
            UpdateProduct(grains, $"1Kg_of_kamut_{seed + 3}", 60);
            UpdateProduct(grains, $"1Kg_of_oats_{seed + 3}", 55);
            UpdateProduct(grains, $"1Kg_of_quinoa_{seed + 3}", 84);
            UpdateProduct(grains, $"1Kg_of_rice_{seed + 3}", 69);
            UpdateProduct(grains, $"1Kg_of_spelt_{seed + 3}", 83);
            UpdateProduct(grains, $"1Kg_of_wild_rice_{seed + 3}", 74);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(grains.Products, 
                    p => Assert.Equal(grains, p.Parent));
        }

        #endregion

        #region Oils products

        var oils = CreateSubcategory(foods, $"Oils_{seed + 2}");
                                                                                                                                                     
        if (tests.Contains(TestType.CategoryParent))
            Assert.Equal(foods, oils.Parent);
 
        if (tests.Contains(TestType.Subcategories))
        {
            Assert.Contains(foods.Subcategories, 
                c => c.Name == $"Oils_{seed + 2}" && c.Parent == foods);
        }
 
        if (tests.Contains(TestType.GetCategory))
        {
            Assert.Equal(oils, 
                inventory.GetCategory($"Foods_{seed + 1}", $"Oils_{seed + 2}"));
        }

        if (tests.Contains(TestType.UpdateProduct))
        {
            UpdateProduct(oils, $"1Lb_of_canola_oil_{seed + 3}", 94);
            UpdateProduct(oils, $"1Lb_of_olive_oil_{seed + 3}", 43);
            UpdateProduct(oils, $"1Lb_of_palm_oil_{seed + 3}", 28);
            UpdateProduct(oils, $"1Lb_of_sesame_oil_{seed + 3}", 24);
            UpdateProduct(oils, $"1Lb_of_sunflower_oil_{seed + 3}", 94);
            UpdateProduct(oils, $"1Lb_of_vegetable_oil_{seed + 3}", 84);

            if (tests.Contains(TestType.ProductParent))
                Assert.All(oils.Products, 
                    p => Assert.Equal(oils, p.Parent));
        }

        #endregion

        #endregion

        return inventory;
    }
}
