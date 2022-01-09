// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;

namespace Shared
{
    // Shared domain data abstractions for menu items

    public abstract record MenuItem(int MenuId, string Description, double Price);

    public abstract record Food(int MenuId, string Description, double Price)
        : MenuItem(MenuId, Description, Price);

    public abstract record Beverage(int MenuId, string Description, double Price)
        : MenuItem(MenuId, Description, Price);
}
