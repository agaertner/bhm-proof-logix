using System;

namespace Nekres.ProofLogix.Core {
    public static class DateTimeExtensions {
        public static string AsTimeAgo(this DateTime dateTime) {
            var timeSpan = DateTime.Now.Subtract(dateTime);

            return timeSpan.TotalSeconds switch {
                <= 2  => "just now",
                <= 4  => "a few seconds ago",
                <= 60 => $"{timeSpan.Seconds} seconds ago",
                _ => timeSpan.TotalMinutes switch {
                    <= 2 => "about a minute ago",
                    < 60 => $"about {timeSpan.Minutes} minutes ago",
                    _ => timeSpan.TotalHours switch {
                        <= 2 => "about an hour ago",
                        < 24 => $"about {timeSpan.Hours} hours ago",
                        _ => timeSpan.TotalDays switch {
                            <= 2       => "yesterday",
                            <= 30      => $"about {timeSpan.Days} days ago",
                            <= 60      => "about a month ago",
                            < 365      => $"about {timeSpan.Days / 30} months ago",
                            <= 365 * 2 => "about a year ago",
                            _          => $"about {timeSpan.Days / 365} years ago"
                        }
                    }
                }
            };
        }
    }
}
