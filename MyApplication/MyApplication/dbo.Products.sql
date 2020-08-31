CREATE TABLE [dbo].[Products] (
    [ProductID]     INT            IDENTITY (1, 1) NOT NULL,
    [Title]         NVARCHAR (50)  NOT NULL,
    [Description]   NVARCHAR (MAX) NOT NULL,
    [ImagePath]     NVARCHAR (MAX) NULL,
    [Price]         NVARCHAR (MAX) NOT NULL,
    [CategoryId]    INT            NOT NULL,
    [UserId]        NVARCHAR (128) NULL,
    [ProductApprove] BIT            NOT NULL,
    CONSTRAINT [PK_dbo.Products] PRIMARY KEY CLUSTERED ([ProductID] ASC),
    CONSTRAINT [FK_dbo.Products_dbo.Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([CategoryId]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.Products_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_CategoryId]
    ON [dbo].[Products]([CategoryId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[Products]([UserId] ASC);

