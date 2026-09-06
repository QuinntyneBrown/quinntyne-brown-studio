using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Exceptions;
using QuinntyneBrownStudio.Domain.Policies;

namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class DiscountAndTimeTests
{
    [Theory]
    [InlineData(89, "0")]
    [InlineData(90, "10")]
    [InlineData(91, "10")]
    public void AC_L2_015_02_Advance_threshold_uses_Toronto_calendar_days(int days, string expected)
    {
        var now = DateTimeOffset.Parse("2027-01-01T23:30:00-05:00");
        var config = new DiscountConfiguration
        {
            AdvanceRule = new()
            {
                Enabled = true,
                Percentage = 10,
                Threshold = 90,
            },
        };
        // Construct the intended Toronto calendar date; adding elapsed days across DST
        // to 23:30 would instead land on the following local date.
        var localSession = Rules.Local(now).Date.AddDays(days).AddHours(12);
        var session = new DateTimeOffset(localSession, Rules.Toronto.GetUtcOffset(localSession));
        var result = DiscountPolicy.Calculate(config, 100, null, session, now);
        Assert.Equal(decimal.Parse(expected), result.Percentage);
    }

    [Fact]
    public void AC_L2_017_01_AC_L2_057_01_Ties_prefer_code_then_advance_then_weekday()
    {
        var now = DateTimeOffset.Parse("2027-01-01T12:00:00-05:00");
        var session = now.AddDays(100);
        var config = new DiscountConfiguration
        {
            AdvanceRule = new() { Enabled = true, Percentage = 10 },
            WeekdayRule = new()
            {
                Enabled = true,
                Percentage = 10,
                Weekdays = [Rules.Local(session).DayOfWeek],
            },
            CodeRules =
            [
                new()
                {
                    Enabled = true,
                    Code = "TEN",
                    Percentage = 10,
                },
            ],
        };
        Assert.Equal("Code", DiscountPolicy.Calculate(config, 100, " ten ", session, now).Kind);
        Assert.Equal("Advance", DiscountPolicy.Calculate(config, 100, null, session, now).Kind);
        config.AdvanceRule.Enabled = false;
        Assert.Equal("Weekday", DiscountPolicy.Calculate(config, 100, null, session, now).Kind);
    }

    [Fact]
    public void AC_L2_014_02_Invalid_code_preserves_other_eligible_discounts()
    {
        var now = DateTimeOffset.Parse("2027-01-01T12:00:00-05:00");
        var config = new DiscountConfiguration
        {
            AdvanceRule = new() { Enabled = true, Percentage = 10 },
        };
        var result = DiscountPolicy.Calculate(config, 99.95m, "INVALID", now.AddDays(100), now);
        Assert.NotNull(result.CodeError);
        Assert.Equal("Advance", result.Kind);
        Assert.Equal(10.00m, result.Amount.Amount);
    }

    [Fact]
    public void AC_L2_058_01_Nonexistent_spring_time_is_rejected_and_both_fall_offsets_are_valid()
    {
        Assert.Throws<StudioException>(() =>
            Rules.Interval(
                DateTimeOffset.Parse("2027-03-14T02:30:00-05:00"),
                DateTimeOffset.Parse("2027-03-14T04:00:00-04:00")
            )
        );
        Rules.Interval(
            DateTimeOffset.Parse("2027-11-07T01:00:00-04:00"),
            DateTimeOffset.Parse("2027-11-07T01:30:00-04:00")
        );
        Rules.Interval(
            DateTimeOffset.Parse("2027-11-07T01:00:00-05:00"),
            DateTimeOffset.Parse("2027-11-07T01:30:00-05:00")
        );
    }
}
