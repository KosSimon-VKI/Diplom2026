BEGIN TRANSACTION;
GO

CREATE TABLE [OrderDrinkItemModifiers] (
    [Id] int NOT NULL IDENTITY,
    [OrderDrinkItemId] int NOT NULL,
    [MilkIngredientId] int NULL,
    [CoffeeIngredientId] int NULL,
    CONSTRAINT [PK_OrderDrinkItemModifiers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OrderDrinkItemModifiers_Ingredients_Coffee] FOREIGN KEY ([CoffeeIngredientId]) REFERENCES [Ingredients] ([Id]),
    CONSTRAINT [FK_OrderDrinkItemModifiers_Ingredients_Milk] FOREIGN KEY ([MilkIngredientId]) REFERENCES [Ingredients] ([Id]),
    CONSTRAINT [FK_OrderDrinkItemModifiers_OrderDrinkItems] FOREIGN KEY ([OrderDrinkItemId]) REFERENCES [OrderDrinkItems] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_OrderDrinkItemModifiers_CoffeeIngredientId] ON [OrderDrinkItemModifiers] ([CoffeeIngredientId]);
GO

CREATE INDEX [IX_OrderDrinkItemModifiers_MilkIngredientId] ON [OrderDrinkItemModifiers] ([MilkIngredientId]);
GO

CREATE UNIQUE INDEX [UX_OrderDrinkItemModifiers_OrderDrinkItemId] ON [OrderDrinkItemModifiers] ([OrderDrinkItemId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260318050658_AddOrderDrinkItemModifiers', N'8.0.0');
GO

COMMIT;
GO

