// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;

namespace Shared.Menu
{
    public record Burger()
        : Food(1, "A third-pound burger with lettuce, tomato, and onion", 3.99);

    public record Fries()
        : Food(2, "A large order of fresh-cut, seasoned french fries", 2.49);

    public record SoftDrink()
        : Beverage(3, "A fountain drink of your choice", 1.79);

    public record Tea()
        : Beverage(4, "Homemade, southern-style, sweetened iced tea", 1.79);

}
