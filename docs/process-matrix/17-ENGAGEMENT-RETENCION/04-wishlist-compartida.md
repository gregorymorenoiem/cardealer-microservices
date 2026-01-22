# 💝 Wishlist Compartida

> **Código:** WISH-001, WISH-002  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🟢 BAJA (Feature social)

---

## 📋 Información General

| Campo             | Valor                            |
| ----------------- | -------------------------------- |
| **Servicio**      | VehiclesSaleService (extendido)  |
| **Puerto**        | 5005                             |
| **Base de Datos** | `vehiclessaleservice`            |
| **Dependencias**  | UserService, NotificationService |

---

## 🎯 Objetivo del Proceso

1. **Colaboración:** Parejas/familias buscando juntos
2. **Compartir hallazgos:** Enviar favoritos a amigos
3. **Decisión grupal:** Votar por vehículos
4. **Viralidad:** Nuevos usuarios desde links compartidos

---

## 📡 Endpoints

| Método   | Endpoint                                           | Descripción               | Auth                |
| -------- | -------------------------------------------------- | ------------------------- | ------------------- |
| `POST`   | `/api/wishlists`                                   | Crear wishlist compartida | ✅                  |
| `GET`    | `/api/wishlists`                                   | Mis wishlists             | ✅                  |
| `GET`    | `/api/wishlists/{id}`                              | Ver wishlist              | ⚠️ Si tiene permiso |
| `GET`    | `/api/wishlists/shared/{shareToken}`               | Ver wishlist pública      | ❌                  |
| `POST`   | `/api/wishlists/{id}/vehicles`                     | Agregar vehículo          | ✅                  |
| `DELETE` | `/api/wishlists/{id}/vehicles/{vehicleId}`         | Quitar vehículo           | ✅                  |
| `POST`   | `/api/wishlists/{id}/collaborators`                | Invitar colaborador       | ✅                  |
| `POST`   | `/api/wishlists/{id}/vehicles/{vehicleId}/vote`    | Votar                     | ✅                  |
| `POST`   | `/api/wishlists/{id}/vehicles/{vehicleId}/comment` | Comentar                  | ✅                  |

---

## 🗃️ Entidades

### SharedWishlist

```csharp
public class SharedWishlist
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; }

    // Info
    public string Name { get; set; }                 // "Carros para la familia"
    public string Description { get; set; }
    public string CoverImageUrl { get; set; }

    // Privacidad
    public WishlistPrivacy Privacy { get; set; }
    public string ShareToken { get; set; }           // Para links públicos
    public string ShareUrl { get; set; }             // okla.do/w/abc123

    // Colaboradores
    public List<WishlistCollaborator> Collaborators { get; set; }

    // Vehículos
    public List<WishlistVehicle> Vehicles { get; set; }
    public int VehicleCount { get; set; }

    // Stats
    public int TotalVotes { get; set; }
    public int TotalComments { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum WishlistPrivacy
{
    Private,            // Solo yo
    Collaborators,      // Solo invitados
    LinkOnly,           // Cualquiera con link
    Public              // Visible en perfil
}
```

### WishlistCollaborator

```csharp
public class WishlistCollaborator
{
    public Guid Id { get; set; }
    public Guid WishlistId { get; set; }
    public Guid? UserId { get; set; }               // Null si invitado por email

    // Invitación
    public string InvitedEmail { get; set; }
    public string InvitedName { get; set; }
    public CollaboratorRole Role { get; set; }
    public CollaboratorStatus Status { get; set; }

    // Permisos
    public bool CanAdd { get; set; }
    public bool CanRemove { get; set; }
    public bool CanVote { get; set; }
    public bool CanComment { get; set; }
    public bool CanInvite { get; set; }

    // Timeline
    public DateTime InvitedAt { get; set; }
    public Guid InvitedBy { get; set; }
    public DateTime? JoinedAt { get; set; }
}

public enum CollaboratorRole
{
    Viewer,         // Solo ver
    Contributor,    // Agregar y votar
    Admin           // Todo excepto eliminar wishlist
}

public enum CollaboratorStatus
{
    Pending,
    Accepted,
    Declined,
    Removed
}
```

### WishlistVehicle

```csharp
public class WishlistVehicle
{
    public Guid Id { get; set; }
    public Guid WishlistId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid AddedBy { get; set; }
    public string AddedByName { get; set; }

    // Datos del vehículo (snapshot)
    public string VehicleTitle { get; set; }
    public string VehicleImage { get; set; }
    public decimal VehiclePrice { get; set; }
    public string VehicleUrl { get; set; }

    // Votación
    public List<WishlistVote> Votes { get; set; }
    public int UpVotes { get; set; }
    public int DownVotes { get; set; }
    public int VoteScore { get; set; }              // Up - Down

    // Comentarios
    public List<WishlistComment> Comments { get; set; }
    public int CommentCount { get; set; }

    // Nota personal
    public string Note { get; set; }

    // Estado
    public VehicleWishlistStatus Status { get; set; }

    public DateTime AddedAt { get; set; }
}

public enum VehicleWishlistStatus
{
    Active,
    Contacted,          // Alguien contactó al vendedor
    TestDriveScheduled, // Agendaron test drive
    Purchased,          // Lo compraron!
    Removed,            // Quitado de la lista
    Sold                // Ya se vendió (no disponible)
}
```

### WishlistVote

```csharp
public class WishlistVote
{
    public Guid Id { get; set; }
    public Guid WishlistVehicleId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }

    public VoteType Type { get; set; }
    public string Reason { get; set; }              // Opcional: "Muy caro"

    public DateTime VotedAt { get; set; }
}

public enum VoteType
{
    Up,     // 👍
    Down    // 👎
}
```

### WishlistComment

```csharp
public class WishlistComment
{
    public Guid Id { get; set; }
    public Guid WishlistVehicleId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string UserAvatar { get; set; }

    public string Content { get; set; }
    public List<string> AttachmentUrls { get; set; }

    public Guid? ReplyToId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
}
```

---

## 📊 Proceso WISH-001: Crear Wishlist Compartida

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: WISH-001 - Crear y Compartir Wishlist                         │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG                                               │
│ Sistemas: VehiclesSaleService, NotificationService                     │
│ Duración: 1-2 minutos                                                  │
│ Criticidad: BAJA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                 | Sistema             | Actor     | Evidencia                | Código     |
| ---- | ------- | -------------------------------------- | ------------------- | --------- | ------------------------ | ---------- |
| 1    | 1.1     | Usuario en página de favoritos         | Frontend            | USR-REG   | Page accessed            | EVD-LOG    |
| 1    | 1.2     | Click "Crear Lista Compartida"         | Frontend            | USR-REG   | CTA clicked              | EVD-LOG    |
| 2    | 2.1     | Modal de nueva wishlist                | Frontend            | USR-REG   | Modal shown              | EVD-SCREEN |
| 2    | 2.2     | Ingresar nombre                        | Frontend            | USR-REG   | Name input               | EVD-LOG    |
| 2    | 2.3     | Seleccionar privacidad                 | Frontend            | USR-REG   | Privacy selected         | EVD-LOG    |
| 3    | 3.1     | POST /api/wishlists                    | Gateway             | USR-REG   | **Request**              | EVD-AUDIT  |
| 3    | 3.2     | Generar ShareToken                     | VehiclesSaleService | Sistema   | Token generated          | EVD-LOG    |
| 3    | 3.3     | **Crear SharedWishlist**               | VehiclesSaleService | Sistema   | **Wishlist created**     | EVD-AUDIT  |
| 4    | 4.1     | Mostrar wishlist creada                | Frontend            | USR-REG   | Wishlist shown           | EVD-SCREEN |
| 4    | 4.2     | Opción "Invitar colaboradores"         | Frontend            | USR-REG   | Invite option            | EVD-SCREEN |
| 5    | 5.1     | Ingresar emails de invitados           | Frontend            | USR-REG   | Emails input             | EVD-LOG    |
| 5    | 5.2     | POST /api/wishlists/{id}/collaborators | Gateway             | USR-REG   | **Request**              | EVD-AUDIT  |
| 5    | 5.3     | **Crear WishlistCollaborator**         | VehiclesSaleService | Sistema   | **Collaborator created** | EVD-AUDIT  |
| 6    | 6.1     | **Enviar invitaciones**                | NotificationService | SYS-NOTIF | **Invites sent**         | EVD-COMM   |
| 6    | 6.2     | Email con link para unirse             | Email               | SYS-NOTIF | Email sent               | EVD-COMM   |
| 7    | 7.1     | Copiar link de wishlist                | Frontend            | USR-REG   | Link copied              | EVD-LOG    |
| 7    | 7.2     | Compartir por WhatsApp/Redes           | Frontend            | USR-REG   | Shared                   | EVD-LOG    |
| 8    | 8.1     | **Audit trail**                        | AuditService        | Sistema   | Complete audit           | EVD-AUDIT  |

### Evidencia de Wishlist Creada

```json
{
  "processCode": "WISH-001",
  "wishlist": {
    "id": "wishlist-12345",
    "owner": {
      "id": "user-001",
      "name": "María García"
    },
    "info": {
      "name": "Opciones para Juan (regalo cumpleaños)",
      "description": "Estamos buscando un carro para mi esposo",
      "privacy": "COLLABORATORS"
    },
    "share": {
      "token": "abc123xyz",
      "url": "https://okla.com.do/w/abc123xyz"
    },
    "collaborators": [
      {
        "email": "hermana@email.com",
        "name": "Ana",
        "role": "CONTRIBUTOR",
        "status": "PENDING",
        "invitedAt": "2026-01-21T10:30:00Z"
      },
      {
        "email": "mama@email.com",
        "name": "Carmen",
        "role": "VIEWER",
        "status": "PENDING",
        "invitedAt": "2026-01-21T10:30:00Z"
      }
    ],
    "stats": {
      "vehicles": 0,
      "collaborators": 2,
      "votes": 0,
      "comments": 0
    },
    "createdAt": "2026-01-21T10:30:00Z"
  }
}
```

---

## 📊 Proceso WISH-002: Votar y Comentar en Wishlist

| Paso | Subpaso | Acción                                                | Sistema             | Actor      | Evidencia           | Código     |
| ---- | ------- | ----------------------------------------------------- | ------------------- | ---------- | ------------------- | ---------- |
| 1    | 1.1     | Colaborador accede a wishlist                         | Frontend            | USR-COLLAB | Wishlist accessed   | EVD-LOG    |
| 1    | 1.2     | Ver vehículos agregados                               | Frontend            | USR-COLLAB | Vehicles shown      | EVD-SCREEN |
| 2    | 2.1     | Click 👍 o 👎 en vehículo                             | Frontend            | USR-COLLAB | Vote clicked        | EVD-LOG    |
| 2    | 2.2     | POST /api/wishlists/{id}/vehicles/{vehicleId}/vote    | Gateway             | USR-COLLAB | **Request**         | EVD-AUDIT  |
| 2    | 2.3     | **Crear/Actualizar WishlistVote**                     | VehiclesSaleService | Sistema    | **Vote recorded**   | EVD-AUDIT  |
| 2    | 2.4     | Actualizar VoteScore                                  | VehiclesSaleService | Sistema    | Score updated       | EVD-LOG    |
| 3    | 3.1     | Click "Comentar"                                      | Frontend            | USR-COLLAB | Comment clicked     | EVD-LOG    |
| 3    | 3.2     | Escribir comentario                                   | Frontend            | USR-COLLAB | Comment input       | EVD-LOG    |
| 3    | 3.3     | POST /api/wishlists/{id}/vehicles/{vehicleId}/comment | Gateway             | USR-COLLAB | **Request**         | EVD-AUDIT  |
| 3    | 3.4     | **Crear WishlistComment**                             | VehiclesSaleService | Sistema    | **Comment created** | EVD-AUDIT  |
| 4    | 4.1     | **Notificar a otros colaboradores**                   | NotificationService | SYS-NOTIF  | **Notifications**   | EVD-COMM   |
| 4    | 4.2     | Push: "Ana votó 👍 por Toyota Corolla"                | Push                | SYS-NOTIF  | Push sent           | EVD-COMM   |
| 5    | 5.1     | Actualizar UI en tiempo real                          | WebSocket           | Sistema    | Real-time update    | EVD-LOG    |

---

## 📱 UI Mockup de Wishlist

```
┌─────────────────────────────────────────────────────────────────────────┐
│ ← Mis Listas    Opciones para Juan 🎂         [👤+2] [📤 Compartir]    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Ordenar por: [Score ▾]    Colaboradores: María(tú), Ana, Carmen       │
│                                                                         │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ 🏆 FAVORITO                                                         │ │
│ │ ┌──────────┐  Toyota Corolla 2023                                  │ │
│ │ │  [IMG]   │  RD$ 1,250,000                        👍 3  👎 0      │ │
│ │ │          │  ─────────────────────────────────                    │ │
│ │ │          │  💬 María: "Este es el que más me gusta"              │ │
│ │ │          │  💬 Ana: "Buen precio, excelente estado"              │ │
│ │ └──────────┘                      [Ver Vehículo] [Contactar]        │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ ┌──────────┐  Honda Civic 2022                                     │ │
│ │ │  [IMG]   │  RD$ 1,150,000                        👍 1  👎 1      │ │
│ │ │          │  ─────────────────────────────────                    │ │
│ │ │          │  💬 Carmen: "Muy deportivo para él 😅"                 │ │
│ │ └──────────┘                      [Ver Vehículo] [Contactar]        │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│  [+ Agregar Vehículo desde Búsqueda]                                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Métricas Prometheus

```yaml
# Wishlists
wishlist_created_total
wishlist_vehicles_added_total
wishlist_shared_total{method}  # link, whatsapp, email
wishlist_collaborators_invited_total
wishlist_collaborators_joined_total

# Engagement
wishlist_votes_total{type}     # up, down
wishlist_comments_total
wishlist_avg_vehicles_per_list

# Conversión
wishlist_to_contact_rate
wishlist_to_purchase_rate
wishlist_viral_signups_total   # Nuevos usuarios desde links
```

---

## 🔗 Referencias

- [03-VEHICULOS-INVENTARIO/04-favorites-service.md](../03-VEHICULOS-INVENTARIO/04-favorites-service.md)
- [07-NOTIFICACIONES/01-notification-service.md](../07-NOTIFICACIONES/01-notification-service.md)
