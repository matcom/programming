
using Xunit;
using System.Linq;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MatCom.Tester;

public class UnitTest
{
    [Fact]
    public void RootCase()
    {
        // create a new inventory and get its root category
        var inventory = Exam.Exam.GetInventory();
        var rootCategory = inventory.Root;

        // verify that the root category is the parent of itself and its name is empty
        Assert.Equal(rootCategory, rootCategory.Parent);
        Assert.Equal("", rootCategory.Name);
    }

    [Theory]
    [InlineData(2894)]
    // [InlineData(492)]
    // [InlineData(4958)]
    // [InlineData(952)]
    // [InlineData(2439)]
    // [InlineData(2984)]
    // [InlineData(492)]
    // [InlineData(405)]
    // [InlineData(112)]
    public void CreateSubcategory(int seed)
    {
        Utils.CreateInventory(seed, new List<TestType> 
            { TestType.CreateSubcategory });
    }

    [Theory]
    [InlineData(240)]
    // [InlineData(2409)]
    // [InlineData(24092)]
    // [InlineData(283)]
    // [InlineData(9403)]
    // [InlineData(2942)]
    // [InlineData(2984)]
    // [InlineData(439)]
    // [InlineData(293)]
    // [InlineData(12377)]
    public void UpdateProduct(int seed)
    {
        Utils.CreateInventory(seed, new List<TestType> 
            { TestType.CreateSubcategory, TestType.UpdateProduct });
    }

    [Theory]
    [InlineData(1)]
    // [InlineData(292)]
    // [InlineData(514)]
    // [InlineData(482)] 
    // [InlineData(582)]
    // [InlineData(2894)]
    // [InlineData(9292)]
    // [InlineData(294)]
    // [InlineData(444)]
    // [InlineData(298)]
    // [InlineData(43)]
    public void GetCategory(int seed)
    {
        Utils.CreateInventory(seed, new List<TestType> 
            { TestType.CreateSubcategory, TestType.GetCategory });
    }

    [Theory]
    [InlineData(4398)]
    // [InlineData(439)]
    // [InlineData(292)]
    // [InlineData(50)]
    // [InlineData(15)]
    // [InlineData(11894)]
    // [InlineData(4781)]
    // [InlineData(192)]
    // [InlineData(289)]
    // [InlineData(5471)]
    public void Subcategories(int seed)
    {
        Utils.CreateInventory(seed, new List<TestType> 
            { TestType.CreateSubcategory, TestType.UpdateProduct, TestType.Subcategories });
    }

    [Theory]
    [InlineData(583)]
    // [InlineData(8592)]
    // [InlineData(20)]
    // [InlineData(415)]
    // [InlineData(221)]
    // [InlineData(228)]
    // [InlineData(243)]
    // [InlineData(340)]
    // [InlineData(209)]
    // [InlineData(4293)]
    public void CategoryParent(int seed)
    {
        Utils.CreateInventory(seed, new List<TestType> 
            { TestType.CreateSubcategory, TestType.CategoryParent });
    }

    [Theory]
    [InlineData(242)]
    // [InlineData(349)]
    // [InlineData(1895)]
    // [InlineData(2951)]
    // [InlineData(195)]
    // [InlineData(1985)]
    // [InlineData(189)]
    // [InlineData(289)]
    // [InlineData(19815)]
    // [InlineData(8349)]
    public void ProductParent(int seed)
    {
        var inventory = Utils.CreateInventory(seed, new List<TestType> 
            { TestType.CreateSubcategory, TestType.UpdateProduct, TestType.ProductParent });
    }

    [Theory]
    [InlineData(1932)]
    // [InlineData(5115)]
    // [InlineData(51)]
    // [InlineData(15641)]
    // [InlineData(2984)]
    // [InlineData(98741)]
    // [InlineData(151)]
    // [InlineData(981)]
    // [InlineData(9126)]
    // [InlineData(8941)]
    public void GetProduct(int seed)
    {
        var inventory = Utils.CreateInventory(seed, new List<TestType> 
            { TestType.CreateSubcategory, TestType.UpdateProduct });
        
        var daewooSplit = Utils.GetProduct(inventory, $"Daewoo_{seed + 3}", 4, 
            $"HomeAppliances_{seed + 1}", $"Splits_{seed + 2}");
        var daewooMicrowave = Utils.GetProduct(inventory, $"Daewoo_{seed + 3}", 4, 
            $"HomeAppliances_{seed + 1}", $"Microwaves_{seed + 2}");
        var daewooFan = Utils.GetProduct(inventory, $"Daewoo_{seed + 3}", 5, 
            $"HomeAppliances_{seed + 1}", $"Fans_{seed + 2}");
        var daewooRefrigerator = Utils.GetProduct(inventory, $"Daewoo_{seed + 3}", 5, 
            $"HomeAppliances_{seed + 1}", $"Refrigerators_{seed + 2}");
        
        Assert.False(daewooSplit == daewooMicrowave);
        Assert.False(daewooMicrowave == daewooFan);
        Assert.False(daewooFan == daewooRefrigerator);

        var samsungFlat = Utils.GetProduct(inventory, $"Samsung_{seed + 4}", 4, 
            $"HomeAppliances_{seed + 1}", $"Televisions_{seed + 2}", 
            $"FlatTvs_{seed + 3}");
        var samsungCurved = Utils.GetProduct(inventory, $"Samsung_{seed + 4}", 4, 
            $"HomeAppliances_{seed + 1}", $"Televisions_{seed + 2}", 
            $"CurvedTvs_{seed + 3}");

        Assert.False(samsungFlat == samsungCurved);     
    }

    [Theory]
    [InlineData(489)]
    // [InlineData(927)]
    // [InlineData(98115)]
    // [InlineData(156)]
    // [InlineData(9811)]
    // [InlineData(6516)]
    // [InlineData(166)]
    // [InlineData(19561)]
    // [InlineData(1981)]
    // [InlineData(9156)]
    public void FindAll(int seed)
    {
        var inventory = Utils.CreateInventory(
            seed, new List<TestType> 
                { TestType.CreateSubcategory, TestType.UpdateProduct });

        Assert.True(2==inventory.FindAll(
            p => p.Name == $"Samsung_{seed + 4}").Count());
        
        Assert.True(5==inventory.FindAll(p => p.Count == 55).Count());
        Assert.True(5==inventory.FindAll(p => p.Count == 83).Count());
        Assert.True(3==inventory.FindAll(p => p.Count == 74).Count());
        Assert.True(9==inventory.FindAll(p => p.Count == 100).Count());
        Assert.True(14==inventory.FindAll(p => p.Count == 4).Count());
        
        Assert.True(38==inventory.FindAll(p => p.Count < 20).Count());
        Assert.True(14==inventory.FindAll(p => p.Count > 13 && 
            p.Count < 47).Count());
        Assert.True(25==inventory.FindAll(p => p.Count > 26 && 
            p.Count < 77).Count());
        Assert.True(35==inventory.FindAll(p => p.Count > 50 && 
            p.Count < 118).Count());
        Assert.True(1==inventory.FindAll(p => p.Count > 100).Count());

        Assert.True(0==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @$"([a-zA-Z0-9]_)+{seed + 1}")).Count());
        Assert.True(0==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @$"([a-zA-Z0-9]_)+{seed + 2}")).Count());
        Assert.True(43==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @$"([a-zA-Z0-9]_)+{seed + 3}")).Count());
        Assert.True(46==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @$"([a-zA-Z0-9]_)+{seed + 4}")).Count());

        Assert.True(2==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"Toshiba_[0-9]+")).Count());
        Assert.True(6==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"Panasonic_[0-9]+")).Count());
        Assert.True(2==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"Midea_[0-9]+")).Count());
        Assert.True(4==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"Philips_[0-9]+")).Count());
        Assert.True(6==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"Samsung_[0-9]+")).Count());
        Assert.True(4==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"Daewoo_[0-9]+")).Count());
        Assert.True(6==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"LG_[0-9]+")).Count());
        Assert.True(1==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"Royal_[0-9]+")).Count());
        Assert.True(4==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"Sony_[0-9]+")).Count());
        Assert.True(1==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"Carrier_[0-9]+")).Count());

        Assert.True(1==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"Royal_[0-9]+")).Count());
        Assert.True(4==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"Sony_[0-9]+")).Count());
        Assert.True(1 == inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"Royal_[0-9]+")).Count());
        Assert.True(4==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"Sony_[0-9]+")).Count());
      
        Assert.True(43==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"1Kg_([a-zA-Z0-9]+_)+")).Count());
        Assert.True(7==inventory.FindAll(
            p => Regex.IsMatch(p.Name, @"1Lb_([a-zA-Z0-9]+_)+")).Count());
    }

    [Theory]
    [InlineData(24)]
    // [InlineData(294)]
    // [InlineData(402)]
    // [InlineData(923)]
    // [InlineData(194)]
    // [InlineData(924)]
    // [InlineData(249)]
    // [InlineData(9035)]
    // [InlineData(5892)]
    // [InlineData(489)]
    public void CreateSubcategoryException(int seed)
    {
        var inventory = Utils.CreateInventory(seed, new List<TestType> 
            { TestType.CreateSubcategory, TestType.UpdateProduct });
        var rootCategory = inventory.Root;

        Assert.ThrowsAny<Exception>(() => {
            rootCategory.CreateSubcategory($"HomeAppliances_{seed + 1}");
        });
        
        var homeAppliances = inventory.GetCategory($"HomeAppliances_{seed + 1}");

        Assert.ThrowsAny<Exception>(() => {
            homeAppliances.CreateSubcategory($"Televisions_{seed + 2}");
        });
    
        var vegetables = inventory.GetCategory($"Foods_{seed + 1}", $"Vegetables_{seed + 2}"); 
        
        // Create the subcategory without problems
        vegetables.CreateSubcategory($"Onions_{seed + 2}");

        Assert.ThrowsAny<Exception>(() => {
            vegetables.CreateSubcategory($"Onions_{seed + 2}");
        });

        Assert.ThrowsAny<Exception>(() => {
            vegetables.CreateSubcategory($"Onions_{seed + 3}");
        });
    }

    [Theory]
    [InlineData(8912)]
    // [InlineData(554)]
    // [InlineData(515)]
    // [InlineData(152)]
    // [InlineData(5165)]
    // [InlineData(87512)]
    // [InlineData(1781)]
    // [InlineData(861)]
    // [InlineData(516)]
    // [InlineData(8941)]
    public void UpdateProductException(int seed)
    {
        var inventory = Utils.CreateInventory(seed, new List<TestType> 
            { TestType.CreateSubcategory, TestType.UpdateProduct });

        var curvedTvs = inventory.GetCategory($"HomeAppliances_{seed + 1}", 
            $"Televisions_{seed + 2}", $"CurvedTvs_{seed + 3}");

        var flatTvs = inventory.GetCategory($"HomeAppliances_{seed + 1}", 
            $"Televisions_{seed + 2}", $"FlatTvs_{seed + 3}");

        curvedTvs.UpdateProduct($"Samsung_{seed + 4}", -3);
        flatTvs.UpdateProduct($"Samsung_{seed + 4}", 3);       
    
        var samsungCurved = inventory.GetProduct($"Samsung_{seed + 4}", 
            $"HomeAppliances_{seed + 1}", $"Televisions_{seed + 2}", 
            $"CurvedTvs_{seed + 3}");
    
        var samsungFlat = inventory.GetProduct($"Samsung_{seed + 4}", 
            $"HomeAppliances_{seed + 1}", $"Televisions_{seed + 2}", 
            $"FlatTvs_{seed + 3}");
    
        Assert.True(samsungCurved.Count == 1);        
        Assert.True(samsungFlat.Count == 7);

        Assert.Throws<ArgumentException>(() => {
            curvedTvs.UpdateProduct("whatever", -1);
        });

        // cannot throw an exception
        flatTvs.UpdateProduct("whatever", 0);
        curvedTvs.UpdateProduct($"Samsung_{seed + 4}", -1);           
        flatTvs.UpdateProduct($"Samsung_{seed + 4}", -7);  

        curvedTvs.UpdateProduct($"Samsung_{seed + 4}", 4);           
        flatTvs.UpdateProduct($"Samsung_{seed + 4}", 2);  

        Assert.ThrowsAny<ArgumentException>(()=>{
            flatTvs.UpdateProduct($"Samsung_{seed + 4}", -3);  
        });

        Assert.ThrowsAny<ArgumentException>(()=>{
            curvedTvs.UpdateProduct($"Samsung_{seed + 4}", -5);           
        });
    }

    [Theory]
    [InlineData(65)]
    // [InlineData(45)]
    // [InlineData(5416)]
    // [InlineData(89512)]
    // [InlineData(51631)]
    // [InlineData(1561)]
    // [InlineData(8910)]
    // [InlineData(78615)]
    // [InlineData(815)]
    // [InlineData(6516)]
    public void GetProductException(int seed)
    {
        var inventory = Utils.CreateInventory(seed, new List<TestType> 
            { TestType.CreateSubcategory, TestType.UpdateProduct });

        var vegetableOil = inventory.GetProduct($"1Lb_of_vegetable_oil_{seed + 3}", 
            $"Foods_{seed + 1}", $"Oils_{seed + 2}");
        var quinoa = inventory.GetProduct($"1Kg_of_quinoa_{seed + 3}", 
            $"Foods_{seed + 1}", $"Grains_{seed + 2}");
        var barley = inventory.GetProduct($"1Kg_of_barley_{seed + 3}", 
            $"Foods_{seed + 1}", $"Grains_{seed + 2}");
        var mackerel = inventory.GetProduct($"1Kg_of_mackerel_{seed + 4}", 
            $"Foods_{seed + 1}", $"Meat_{seed + 2}", $"Fish_{seed + 3}");
        
        var oilCategory = inventory.GetCategory(
            $"Foods_{seed + 1}", $"Oils_{seed + 2}");
        var grainCategory = inventory.GetCategory(
            $"Foods_{seed + 1}", $"Grains_{seed + 2}");
        var fishCategory = inventory.GetCategory($"Foods_{seed + 1}", 
            $"Meat_{seed + 2}", $"Fish_{seed + 3}");
        
        oilCategory.UpdateProduct($"1Lb_of_vegetable_oil_{seed + 3}", -84);
        grainCategory.UpdateProduct($"1Kg_of_quinoa_{seed + 3}", -84);
        grainCategory.UpdateProduct($"1Kg_of_barley_{seed + 3}", -55);
        fishCategory.UpdateProduct($"1Kg_of_mackerel_{seed + 4}", -31);
        fishCategory.UpdateProduct("whatever", 0);

        Assert.Equal(mackerel , inventory.GetProduct($"1Kg_of_mackerel_{seed +4}", 
            $"Foods_{seed + 1}", $"Meat_{seed + 2}", $"Fish_{seed + 3}"));

        fishCategory.UpdateProduct($"1Kg_of_mackerel_{seed + 4}", -1);

        Assert.True(vegetableOil.Count == 0);
        Assert.True(quinoa.Count == 0);
        Assert.True(barley.Count == 0);
        Assert.True(mackerel.Count == 0);

        Assert.Throws<ArgumentException>(() => { 
            inventory.GetProduct($"1Lb_of_vegetable_oil_{seed + 3}", 
            $"Foods_{seed + 1}", $"Oils_{seed + 2}");   
        });
        Assert.Throws<ArgumentException>(() => { 
            inventory.GetProduct($"1Kg_of_quinoa_{seed + 3}", 
            $"Foods_{seed + 1}", $"Grains_{seed + 2}");   
        });
        Assert.Throws<ArgumentException>(() => {
            inventory.GetProduct($"1Kg_of_barley_{seed + 3}", 
            $"Foods_{seed + 1}", $"Grains_{seed + 2}");   
        });
        Assert.Throws<ArgumentException>(() => {
            inventory.GetProduct($"1Kg_of_mackerel_{seed + 4}", 
            $"Foods_{seed + 1}", $"Meat_{seed + 2}", $"Fish_{seed + 3}");   
        });
        Assert.Throws<ArgumentException>(() => {
            inventory.GetProduct("whatever", 
            $"Foods_{seed + 1}", $"Meat_{seed + 2}", $"Fish_{seed + 3}");   
        });
        Assert.Throws<ArgumentException>(() => {
            inventory.GetProduct($"1Kg_of_salmon_{seed + 4}", 
            $"Foods_{seed + 1}", $"meat_{seed + 2}", $"Fish_{seed + 3}");   
        });
        Assert.Throws<ArgumentException>(() => {
            inventory.GetProduct($"1Kg_of_pork_chops_{seed + 4}", 
            $"Foods_{seed + 1}", $"Pork_{seed + 3}");   
        });
        Assert.Throws<ArgumentException>(() => {
            inventory.GetProduct($"1Kg_of_beef_steak_{seed + 4}", 
            $"Foods_{seed + 1}", $"Meat_{seed + 2}");   
        });
        Assert.Throws<ArgumentException>(() => {
            inventory.GetProduct($"1Kg_bag_of_powdered_milk_{seed + 4}", 
            $"Dairy_{seed + 2}", $"Milk_{seed + 3}");   
        });
    }

    [Theory]
    [InlineData(5844)]
    // [InlineData(151)]
    // [InlineData(8888)]
    // [InlineData(865)]
    // [InlineData(21684)]
    // [InlineData(6841)]
    // [InlineData(611)]
    // [InlineData(651)]
    // [InlineData(1651)]
    // [InlineData(489)]
    public void GetCategoryException(int seed)
    {
        var inventory = Utils.CreateInventory(
            seed, new List<TestType> 
                { TestType.CreateSubcategory, TestType.UpdateProduct });
        
        // case in which the subcategories exist but are requested 
        // in an unordered fashion
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"HomeAppliances_{seed + 1}", 
                $"CurvedTvs_{seed + 3}", $"Televisions_{seed + 2}"));
        
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory( $"FlatTvs_{seed + 3}", 
                $"Televisions_{seed + 2}", $"HomeAppliances_{seed + 1}"));
        
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Fruits_{seed + 2}", 
                $"Oranges_{seed + 3}", $"Foods_{seed + 1}"));
        
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Apples_{seed + 3}",
                $"Foods_{seed + 1}", $"Fruits_{seed + 2}"));
        
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Vegetables_{seed + 2}", 
                $"Foods_{seed + 1}", $"Onions_{seed + 3}"));
        
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Foods_{seed + 1}",
                $"Fish_{seed + 3}", $"Meat_{seed + 2}"));
          
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Pork_{seed + 3}",
                $"Foods_{seed + 1}", $"Meat_{seed + 2}"));
       
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Meat_{seed + 2}", 
                $"Foods_{seed + 1}", $"Beef_{seed + 3}"));
       
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Meat_{seed + 2}", 
                $"Chicken_{seed + 3}", $"Foods_{seed + 1}"));
       
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Cheese_{seed + 3}",
                $"Dairy_{seed + 2}", $"Foods_{seed + 1}"));
        
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Yogurt_{seed + 3}",
                $"Dairy_{seed + 2}", $"Foods_{seed + 1}"));
      
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Milk_{seed + 3}",
                $"Foods_{seed + 1}", $"Dairy_{seed + 2}"));

        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Grapes_{seed + 3}", 
                $"Fruits_{seed + 2}", $"Foods_{seed + 1}"));

        // case in which a correct route is entered but 
        // there is some product in the route
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"HomeAppliances_{seed + 1}", 
                $"Televisions_{seed + 2}", $"CurvedTvs_{seed + 3}",
                $"Samsung_{seed + 4}"));

         Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Foods_{seed + 1}", 
                $"Fruits_{seed + 2}", $"1Kg_of_blood_orange_{seed + 4}",
                $"Grapes_{seed + 3}"));

         Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Foods_{seed + 1}", 
                $"1Kg_of_mandarin_{seed + 4}", $"Fruits_{seed + 2}", 
                $"Oranges_{seed + 3}"));

         Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"1Kg_of_green_apples_{seed + 4}", 
                $"Foods_{seed + 1}", $"Fruits_{seed + 2}", 
                $"Oranges_{seed + 3}"));
        
        // case in which a subcategory is requested that does not exist
        // (the correct form is ´FlatTvs_{seed + 3}´)        
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"HomeAppliances_{seed + 1}", 
                $"Televisions_{seed + 2}", $"flatTvs_{seed + 3}"));
 
         Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Foods_{seed + 1}", 
                $"Meat_{seed + 2}", $"Kawama_{seed + 3}"));

         Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Foods_{seed + 1}", 
                $"Dairy_{seed + 2}", $"Blue_cheese_{seed + 3}"));

        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Sports_{seed + 1}", 
                $"Cycling_{seed + 2}"));

        // case in which an intermediate subcategory is missing
        Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory($"Foods_{seed + 1}", // $"Fruits_{seed + 2}", 
                $"Grapes_{seed + 3}"));
  
         Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory(
                $"Foods_{seed + 1}", // $"Dairy_{seed + 2}", 
                $"Cheese_{seed + 3}"));
    
         Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory(//$"HomeAppliances_{seed + 1}", 
                $"Televisions_{seed + 2}", $"FlatTvs_{seed + 3}"));
    
         Assert.Throws<ArgumentException>( 
            () => inventory.GetCategory(// $"Foods_{seed + 1}", 
                $"Vegetables_{seed + 2}", $"Onions_{seed + 3}"    
            ));
    }

    [Theory]
    [InlineData(321)]
    // [InlineData(654)]
    // [InlineData(115)]
    // [InlineData(8544)]
    // [InlineData(556)]
    // [InlineData(111)]
    // [InlineData(455)]
    // [InlineData(894)]
    // [InlineData(8965)]
    // [InlineData(788)]
    public void CombinedTest1(int seed)
    {
        var inventory = Utils.CreateInventory(
            seed, new List<TestType>());

        var rootCategory = inventory.Root;

        var subcategory1 = rootCategory.CreateSubcategory("the same name");
        subcategory1.UpdateProduct("the same name", 1);

        var subcategory2 = subcategory1.CreateSubcategory("the same name");
        subcategory2.UpdateProduct("the same name", 1);

        var subcategory3 = subcategory2.CreateSubcategory("the same name");
        subcategory3.UpdateProduct("the same name", 1);

        var subcategory4 = subcategory3.CreateSubcategory("the same name");
        subcategory4.UpdateProduct("the same name", 0);

        Assert.False(
            subcategory1 == inventory
                .GetCategory("the same name", "the same name")
        );
        Assert.False(
            subcategory1 == inventory
                .GetCategory(
                    "the same name", "the same name", 
                    "the same name"
                )
        );
        Assert.False(
            subcategory1 == inventory
                .GetCategory(
                    "the same name", "the same name", 
                    "the same name", "the same name"
                )
        );
        Assert.False(
            subcategory2 == inventory
                .GetCategory(
                    "the same name", "the same name", 
                    "the same name"
                )
        );
        Assert.False(
            subcategory2 == inventory
                .GetCategory(
                    "the same name", "the same name",
                    "the same name", "the same name" 
                )
        );
        Assert.False(
            subcategory3 == inventory
                .GetCategory(
                    "the same name", "the same name", 
                    "the same name", "the same name"
                )
        );
    }

    [Theory]
    [InlineData(4811)]
    // [InlineData(1981)]
    // [InlineData(165)]
    // [InlineData(5611)]
    // [InlineData(4651)]
    // [InlineData(1651)]
    // [InlineData(891)]
    // [InlineData(415)]
    // [InlineData(8941)]
    // [InlineData(489)]
    public void CombinedTest2(int seed)
    {
        var inventory = Utils.CreateInventory(
            seed, new List<TestType>() 
                { TestType.CreateSubcategory, TestType.UpdateProduct });

        var products1 = inventory.FindAll(p => p.Count > 99);

        Assert.True(products1.Count() == 10);

        foreach (var product in products1)
            product.Parent.UpdateProduct(product.Name, -100); 
            
        Assert.True(products1.Count() == 1);
    }

    [Theory]
    [InlineData(1567)]
    // [InlineData(193)]
    // [InlineData(516)]
    // [InlineData(4621)]
    // [InlineData(223)]
    // [InlineData(489)]
    // [InlineData(45)]
    // [InlineData(354)]
    // [InlineData(235)]
    // [InlineData(4891)]
    public void CombinedTest3(int seed)
    {
        var inventory = Utils.CreateInventory(
            seed, new List<TestType> 
                { TestType.CreateSubcategory, TestType.UpdateProduct });
        
        var splits = inventory.FindAll(
            p => Utils.IsDescendantOf(
                p, inventory.GetCategory(
                    $"HomeAppliances_{seed + 1}",  
                    $"Televisions_{seed + 2}")
        ));

        Assert.True(splits.Count() == 12);

        var curved = inventory.GetCategory(
            $"HomeAppliances_{seed + 1}", $"Televisions_{seed + 2}",
            $"CurvedTvs_{seed + 3}"
        );

        curved.UpdateProduct($"Sony_{seed + 4}", -3);

        Assert.True(2 == splits
            .Where(p => p.Name == $"Samsung_{seed + 4}")
            .Count());

        Assert.True(2 == splits
            .Where(p => p.Name == $"LG_{seed + 4}")
            .Count());

        Assert.True(1 == splits
            .Where(p => p.Name == $"Sony_{seed + 4}")
            .Count());

        Assert.True(2 == splits
            .Where(p => p.Name == $"Panasonic_{seed + 4}")
            .Count());

        Assert.True(2 == splits
            .Where(p => p.Name == $"Toshiba_{seed + 4}")
            .Count());

        Assert.True(2 == splits
            .Where(p => p.Name == $"Philips_{seed + 4}")
            .Count());

        curved.UpdateProduct($"Panasonic_{seed + 4}", -1);

        Assert.True(splits.Count() == 10);
    }
}

