using System.Linq;
using Hazelnut;

namespace Hazelnut
{
    public class BindableString : Bindable<string>
    {
        public readonly Bindable<string> State = new();

        public BindableString() => State.Bind(this, x => "skeleton".OnlyWhen(x.HasValue() && x.All(c => c.IsAnyOf(' ', '▅'))));
    }
}