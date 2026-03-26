using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GardenNookApi.Entities;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ClientCategory> ClientCategories { get; set; }

    public virtual DbSet<Discount> Discounts { get; set; }

    public virtual DbSet<Dish> Dishes { get; set; }

    public virtual DbSet<DishCategory> DishCategories { get; set; }

    public virtual DbSet<DishTopping> DishToppings { get; set; }

    public virtual DbSet<Drink> Drinks { get; set; }

    public virtual DbSet<DrinkCategory> DrinkCategories { get; set; }

    public virtual DbSet<DrinkTopping> DrinkToppings { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<IngredientCategory> IngredientCategories { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<OrderDishItem> OrderDishItems { get; set; }

    public virtual DbSet<OrderDrinkItem> OrderDrinkItems { get; set; }

    public virtual DbSet<OrderDrinkItemModifier> OrderDrinkItemModifiers { get; set; }

    public virtual DbSet<OrderToppingItem> OrderToppingItems { get; set; }

    public virtual DbSet<OrderType> OrderTypes { get; set; }

    public virtual DbSet<Preparation> Preparations { get; set; }

    public virtual DbSet<SemiFinished> SemiFinisheds { get; set; }

    public virtual DbSet<SemiFinishedCategory> SemiFinishedCategories { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<StaffRole> StaffRoles { get; set; }

    public virtual DbSet<TechnicalCard> TechnicalCards { get; set; }

    public virtual DbSet<TechnicalCardIngredientComposition> TechnicalCardIngredientCompositions { get; set; }

    public virtual DbSet<TechnicalCardSemiFinishedComposition> TechnicalCardSemiFinishedCompositions { get; set; }

    public virtual DbSet<ToppingCategory> ToppingCategories { get; set; }

    public virtual DbSet<ToppingsAndSyrup> ToppingsAndSyrups { get; set; }

    public virtual DbSet<UnitsOfMeasure> UnitsOfMeasures { get; set; }

   

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clients__3214EC07C44E7277");

            entity.HasIndex(e => e.PhoneNumber, "UQ_Clients_PhoneNumber").IsUnique();

            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);

            entity.HasOne(d => d.ClientCategory).WithMany(p => p.Clients)
                .HasForeignKey(d => d.ClientCategoryId)
                .HasConstraintName("FK_Clients_ClientCategories");
        });

        modelBuilder.Entity<ClientCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClientCa__3214EC075DD76994");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Discount__3214EC077C9057B9");

            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Dish>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Dishes__3214EC07F3AB62F1");

            entity.Property(e => e.CaloriesKcal).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.CarbsG).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.CostPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CostRub).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FatsG).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Kilojoules).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.MarkupPercent).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.PriceRub).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProteinsG).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);

            entity.HasOne(d => d.Category).WithMany(p => p.Dishes)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Dishes_DishCategories");

            entity.HasOne(d => d.TechnicalCard).WithMany(p => p.Dishes)
                .HasForeignKey(d => d.TechnicalCardId)
                .HasConstraintName("FK_Dishes_TechnicalCards");

            entity.HasOne(d => d.UnitOfMeasure).WithMany(p => p.Dishes)
                .HasForeignKey(d => d.UnitOfMeasureId)
                .HasConstraintName("FK_Dishes_UnitsOfMeasure");
        });

        modelBuilder.Entity<DishCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DishCate__3214EC074FB42ABA");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<DishTopping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DishTopp__3214EC0728CBD9E9");

            entity.Property(e => e.FinalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.OrderDishItem).WithMany(p => p.DishToppings)
                .HasForeignKey(d => d.OrderDishItemId)
                .HasConstraintName("FK_DishToppings_OrderDishItems");

            entity.HasOne(d => d.Topping).WithMany(p => p.DishToppings)
                .HasForeignKey(d => d.ToppingId)
                .HasConstraintName("FK_DishToppings_Toppings");
        });

        modelBuilder.Entity<Drink>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Drinks__3214EC0711C11292");

            entity.Property(e => e.CaloriesKcal).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.CarbsG).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.CostPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CostRub).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FatsG).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Kilojoules).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.MarkupPercent).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.PriceRub).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProteinsG).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);

            entity.HasOne(d => d.Category).WithMany(p => p.Drinks)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Drinks_DrinkCategories");

            entity.HasOne(d => d.TechnicalCard).WithMany(p => p.Drinks)
                .HasForeignKey(d => d.TechnicalCardId)
                .HasConstraintName("FK_Drinks_TechnicalCards");

            entity.HasOne(d => d.UnitOfMeasure).WithMany(p => p.Drinks)
                .HasForeignKey(d => d.UnitOfMeasureId)
                .HasConstraintName("FK_Drinks_UnitsOfMeasure");
        });

        modelBuilder.Entity<DrinkCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DrinkCat__3214EC071FC1B902");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<DrinkTopping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DrinkTop__3214EC079A8C6397");

            entity.Property(e => e.FinalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.OrderDrinkItem).WithMany(p => p.DrinkToppings)
                .HasForeignKey(d => d.OrderDrinkItemId)
                .HasConstraintName("FK_DrinkToppings_OrderDrinkItems");

            entity.HasOne(d => d.Topping).WithMany(p => p.DrinkToppings)
                .HasForeignKey(d => d.ToppingId)
                .HasConstraintName("FK_DrinkToppings_Toppings");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ingredie__3214EC07E34616FD");

            entity.Property(e => e.CostRub).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Stock).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.Ingredients)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Ingredients_IngredientCategories");

            entity.HasOne(d => d.UnitOfMeasure).WithMany(p => p.Ingredients)
                .HasForeignKey(d => d.UnitOfMeasureId)
                .HasConstraintName("FK_Ingredients_UnitsOfMeasure");
        });

        modelBuilder.Entity<IngredientCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ingredie__3214EC079A4C52A6");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventor__3214EC072D2A73E6");

            entity.ToTable("Inventory");

            entity.Property(e => e.CostRub).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Stock).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.UnitOfMeasure).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.UnitOfMeasureId)
                .HasConstraintName("FK_Inventory_UnitsOfMeasure");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Orders__3214EC0762C21CEE");

            entity.Property(e => e.PickupAt).HasColumnType("datetime2");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.TotalCalories).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Client).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("FK_Orders_Clients");

            entity.HasOne(d => d.Discount).WithMany(p => p.Orders)
                .HasForeignKey(d => d.DiscountId)
                .HasConstraintName("FK_Orders_Discounts");

            entity.HasOne(d => d.Status).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_Orders_OrderStatuses");

            entity.HasOne(d => d.OrderType).WithMany(p => p.Orders)
                .HasForeignKey(d => d.OrderTypeId)
                .HasConstraintName("FK_Orders_OrderTypes");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<OrderDishItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderDis__3214EC075A82A4EF");

            entity.Property(e => e.FinalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.Dish).WithMany(p => p.OrderDishItems)
                .HasForeignKey(d => d.DishId)
                .HasConstraintName("FK_OrderDishItems_Dishes");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDishItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderDishItems_Orders");
        });

        modelBuilder.Entity<OrderDrinkItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderDri__3214EC07032F8B5B");

            entity.Property(e => e.FinalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.Drink).WithMany(p => p.OrderDrinkItems)
                .HasForeignKey(d => d.DrinkId)
                .HasConstraintName("FK_OrderDrinkItems_Drinks");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDrinkItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderDrinkItems_Orders");

            entity.HasOne(d => d.OrderDrinkItemModifier).WithOne(p => p.OrderDrinkItem)
                .HasForeignKey<OrderDrinkItemModifier>(d => d.OrderDrinkItemId)
                .HasConstraintName("FK_OrderDrinkItemModifiers_OrderDrinkItems");
        });

        modelBuilder.Entity<OrderDrinkItemModifier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_OrderDrinkItemModifiers");

            entity.ToTable("OrderDrinkItemModifiers");

            entity.HasIndex(e => e.OrderDrinkItemId, "UX_OrderDrinkItemModifiers_OrderDrinkItemId").IsUnique();

            entity.HasOne(d => d.CoffeeIngredient).WithMany(p => p.OrderDrinkItemModifiersCoffeeIngredients)
                .HasForeignKey(d => d.CoffeeIngredientId)
                .HasConstraintName("FK_OrderDrinkItemModifiers_Ingredients_Coffee");

            entity.HasOne(d => d.MilkIngredient).WithMany(p => p.OrderDrinkItemModifiersMilkIngredients)
                .HasForeignKey(d => d.MilkIngredientId)
                .HasConstraintName("FK_OrderDrinkItemModifiers_Ingredients_Milk");
        });

        modelBuilder.Entity<OrderToppingItem>(entity =>
        {
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderToppingItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderToppingItems_Orders");

            entity.HasOne(d => d.Topping).WithMany(p => p.OrderToppingItems)
                .HasForeignKey(d => d.ToppingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderToppingItems_Toppings");
        });

        modelBuilder.Entity<OrderType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderTyp__3214EC078DA72059");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Preparation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Preparat__3214EC07D64E207C");

            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.StockGrams).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.SemiFinished).WithMany(p => p.Preparations)
                .HasForeignKey(d => d.SemiFinishedId)
                .HasConstraintName("FK_Preparations_SemiFinished");
        });

        modelBuilder.Entity<SemiFinished>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SemiFini__3214EC075EDA3B4D");

            entity.ToTable("SemiFinished");

            entity.Property(e => e.CaloriesKcal).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.CarbsG).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.CostRub).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FatsG).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Kilojoules).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.ProteinsG).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.SemiFinisheds)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_SemiFinished_SemiFinishedCategories");

            entity.HasOne(d => d.TechnicalCard).WithMany(p => p.SemiFinisheds)
                .HasForeignKey(d => d.TechnicalCardId)
                .HasConstraintName("FK_SemiFinished_TechnicalCards");

            entity.HasOne(d => d.UnitOfMeasure).WithMany(p => p.SemiFinisheds)
                .HasForeignKey(d => d.UnitOfMeasureId)
                .HasConstraintName("FK_SemiFinished_UnitsOfMeasure");
        });

        modelBuilder.Entity<SemiFinishedCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SemiFini__3214EC0732760261");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Staff__3214EC07E332FC49");

            entity.HasIndex(e => e.Login, "UQ_Staff_Login").IsUnique();

            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Login).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);

            entity.HasOne(d => d.Role).WithMany(p => p.Staff)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_Staff_StaffRoles");
        });

        modelBuilder.Entity<StaffRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StaffRol__3214EC07FFE07C44");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<TechnicalCard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Technica__3214EC07C2B4B83C");

            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<TechnicalCardIngredientComposition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Technica__3214EC07EF4FD960");

            entity.ToTable("TechnicalCardIngredientComposition");

            entity.Property(e => e.ColdLossPercent).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.GrossWeight).HasColumnType("decimal(10, 6)");
            entity.Property(e => e.HotLossPercent).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.NetWeight).HasColumnType("decimal(10, 6)");
            entity.Property(e => e.OutputWeight).HasColumnType("decimal(10, 6)");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.TechnicalCardIngredientCompositions)
                .HasForeignKey(d => d.IngredientId)
                .HasConstraintName("FK_TechCardIngComp_Ingredients");

            entity.HasOne(d => d.TechnicalCard).WithMany(p => p.TechnicalCardIngredientCompositions)
                .HasForeignKey(d => d.TechnicalCardId)
                .HasConstraintName("FK_TechCardIngComp_TechnicalCards");

            entity.HasOne(d => d.UnitOfMeasure).WithMany(p => p.TechnicalCardIngredientCompositions)
                .HasForeignKey(d => d.UnitOfMeasureId)
                .HasConstraintName("FK_TechCardIngComp_UnitsOfMeasure");
        });

        modelBuilder.Entity<TechnicalCardSemiFinishedComposition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Technica__3214EC0776B80680");

            entity.ToTable("TechnicalCardSemiFinishedComposition");

            entity.Property(e => e.ColdLossPercent).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.GrossWeight).HasColumnType("decimal(10, 6)");
            entity.Property(e => e.HotLossPercent).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.NetWeight).HasColumnType("decimal(10, 6)");
            entity.Property(e => e.OutputWeight).HasColumnType("decimal(10, 6)");

            entity.HasOne(d => d.SemiFinished).WithMany(p => p.TechnicalCardSemiFinishedCompositions)
                .HasForeignKey(d => d.SemiFinishedId)
                .HasConstraintName("FK_TechCardSemiComp_SemiFinished");

            entity.HasOne(d => d.TechnicalCard).WithMany(p => p.TechnicalCardSemiFinishedCompositions)
                .HasForeignKey(d => d.TechnicalCardId)
                .HasConstraintName("FK_TechCardSemiComp_TechnicalCards");

            entity.HasOne(d => d.UnitOfMeasure).WithMany(p => p.TechnicalCardSemiFinishedCompositions)
                .HasForeignKey(d => d.UnitOfMeasureId)
                .HasConstraintName("FK_TechCardSemiComp_UnitsOfMeasure");
        });

        modelBuilder.Entity<ToppingCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ToppingC__3213E83FF7AB5EB1");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<ToppingsAndSyrup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Toppings__3214EC07D7C64F9B");

            entity.Property(e => e.CaloriesKcal).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.CarbsG).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CostPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CostRub).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FatsG).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Kilojoules).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.MarkupPercent).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.PriceRub).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProteinsG).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);

            entity.HasOne(d => d.Category).WithMany(p => p.ToppingsAndSyrups)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__ToppingsA__Categ__29221CFB");

            entity.HasOne(d => d.TechnicalCard).WithMany(p => p.ToppingsAndSyrups)
                .HasForeignKey(d => d.TechnicalCardId)
                .HasConstraintName("FK_Toppings_TechnicalCards");

            entity.HasOne(d => d.UnitOfMeasure).WithMany(p => p.ToppingsAndSyrups)
                .HasForeignKey(d => d.UnitOfMeasureId)
                .HasConstraintName("FK_Toppings_UnitsOfMeasure");
        });

        modelBuilder.Entity<UnitsOfMeasure>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UnitsOfM__3214EC0743DE010E");

            entity.ToTable("UnitsOfMeasure");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
