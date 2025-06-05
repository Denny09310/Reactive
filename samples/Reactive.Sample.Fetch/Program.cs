using Reactive.Core;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using static Reactive.Core.Extensions.Reactivity;
using static Reactive.Core.Utils.Tracker;

var id = Signal.State(1);
var resource = Resource(() => new FetchUserArgs(id.Get()), FetchUserAsync);

Effect(() =>
{
    var parameters = Untracked(() => resource.Args.Get());
    var status = resource.Status.Get();
    var value = resource.Value.Get()!;

    switch (status)
    {
        case ResourceState.Loading:
            Console.WriteLine("Loading user with id {0}", parameters.Id);
            break;

        case ResourceState.Refreshing:
            Console.WriteLine("Refreshing user for id {0}. Showing stale {1}", parameters.Id, value.Id);
            break;

        case ResourceState.Success:
            Console.WriteLine("Fetched user with id {0}. Name: '{1} {2}'", value.Id, value.FirstName, value.LastName);
            break;

        case ResourceState.Error:
            Console.WriteLine("Error while fetching user with id {0}. Error: '{1}'", parameters.Id, resource.Error.Get()!.Message);
            break;
    }
});

await Task.Delay(1000);

Console.WriteLine("Changing user id to 4");
id.Set(4);

await Task.Delay(1000);

Console.WriteLine("Changing user id to 6");
id.Set(6);

await Task.Delay(1000);

static async Task<User> FetchUserAsync(FetchUserArgs parameters, CancellationToken ct)
{
    using var http = new HttpClient();
    return await http.GetFromJsonAsync<User>($"https://dummyjson.com/users/{parameters.Id}", ct)
        ?? throw new InvalidOperationException($"Cannot deserialize {nameof(User)}");
}

internal record FetchUserArgs(
    int Id
);

internal record Address(
    [property: JsonPropertyName("address")] string Addr,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("stateCode")] string StateCode,
    [property: JsonPropertyName("postalCode")] string PostalCode,
    [property: JsonPropertyName("coordinates")] Coordinates Coordinates,
    [property: JsonPropertyName("country")] string Country
);

internal record Bank(
    [property: JsonPropertyName("cardExpire")] string CardExpire,
    [property: JsonPropertyName("cardNumber")] string CardNumber,
    [property: JsonPropertyName("cardType")] string CardType,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("iban")] string Iban
);

internal record Company(
    [property: JsonPropertyName("department")] string Department,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("address")] Address Address
);

internal record Coordinates(
    [property: JsonPropertyName("lat")] double Lat,
    [property: JsonPropertyName("lng")] double Lng
);

internal record Crypto(
    [property: JsonPropertyName("coin")] string Coin,
    [property: JsonPropertyName("wallet")] string Wallet,
    [property: JsonPropertyName("network")] string Network
);

internal record Hair(
    [property: JsonPropertyName("color")] string Color,
    [property: JsonPropertyName("type")] string Type
);

internal record User(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("maidenName")] string MaidenName,
    [property: JsonPropertyName("age")] int Age,
    [property: JsonPropertyName("gender")] string Gender,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("phone")] string Phone,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("birthDate")] string BirthDate,
    [property: JsonPropertyName("image")] string Image,
    [property: JsonPropertyName("bloodGroup")] string BloodGroup,
    [property: JsonPropertyName("height")] double Height,
    [property: JsonPropertyName("weight")] double Weight,
    [property: JsonPropertyName("eyeColor")] string EyeColor,
    [property: JsonPropertyName("hair")] Hair Hair,
    [property: JsonPropertyName("ip")] string Ip,
    [property: JsonPropertyName("address")] Address Address,
    [property: JsonPropertyName("macAddress")] string MacAddress,
    [property: JsonPropertyName("university")] string University,
    [property: JsonPropertyName("bank")] Bank Bank,
    [property: JsonPropertyName("company")] Company Company,
    [property: JsonPropertyName("ein")] string Ein,
    [property: JsonPropertyName("ssn")] string Ssn,
    [property: JsonPropertyName("userAgent")] string UserAgent,
    [property: JsonPropertyName("crypto")] Crypto Crypto,
    [property: JsonPropertyName("role")] string Role
);