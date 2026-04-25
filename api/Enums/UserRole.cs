namespace KeycloakAuth.Enums;

/// <summary>
/// Represents the roles available in the Adega Royal system, aligned with Keycloak realm roles.
/// </summary>
public enum UserRole
{
    /// <summary>Customer role with read-only access to products and their own data.</summary>
    Customer = 0,

    /// <summary>Admin role with full access to products, orders, and deliveries.</summary>
    Admin = 1
}
