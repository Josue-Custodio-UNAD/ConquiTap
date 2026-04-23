# ConquiTap 🏕
### Sistema de Gestión de Clubes — ADONE
*Asociación Dominicana del Nordeste — Iglesia Adventista del Séptimo Día*

---

## Requisitos previos

| Herramienta | Versión mínima |
|---|---|
| Windows | 10 / 11 |
| .NET SDK | 6.0 o superior |
| SQL Server | 2019 / 2022 / Express |
| SQL Server Management Studio | 20 |
| Visual Studio | 2022 (recomendado) |

---

## 1. Configurar la base de datos

1. Abrir **SQL Server Management Studio 20**
2. Conectarse a tu instancia (ej: `localhost\SQLEXPRESS`)
3. Abrir el archivo `ConquiTap.sql` desde el menú **File → Open → File...**
4. Ejecutar todo el script con **F5**
5. Verificar que se creó la base de datos `ConquiTap` con todas las tablas

---

## 2. Configurar la cadena de conexión

Abrir `App.config` y modificar el atributo `connectionString` según tu instancia:

```xml
<add name="ConquiTapDB"
     connectionString="Server=TU_SERVIDOR;Database=ConquiTap;Trusted_Connection=True;TrustServerCertificate=True;"
     providerName="Microsoft.Data.SqlClient" />
```

**Ejemplos de `Server`:**
- `localhost\SQLEXPRESS`  → SQL Server Express local
- `.\SQLEXPRESS`          → Forma corta del anterior
- `NOMBREPC\SQLSERVER`    → Instancia nombrada en otro equipo
- `localhost`             → SQL Server en puerto estándar

---

## 3. Abrir y compilar el proyecto

### Con Visual Studio 2022
1. Abrir la carpeta del proyecto o el archivo `ConquiTap.csproj`
2. Seleccionar la configuración **Debug** o **Release**
3. Presionar **F5** para compilar y ejecutar

### Con línea de comandos (.NET CLI)
```bash
cd ConquiTap
dotnet restore
dotnet run
```

---

## 4. Primera ejecución — Crear el administrador

Al ejecutar por primera vez, el sistema detectará que no existe ningún administrador y mostrará el **asistente de configuración inicial**:

1. Ingresar nombre de usuario para el admin (ej: `admin`)
2. Ingresar correo (opcional)
3. Ingresar contraseña (mínimo 6 caracteres)
4. Confirmar contraseña
5. Hacer clic en **"Crear Administrador"**

---

## 5. Iniciar sesión

Usar las credenciales del administrador recién creado.

---

## Estructura del proyecto

```
ConquiTap/
├── App.config                  # Cadena de conexión
├── ConquiTap.csproj            # Archivo de proyecto .NET 6
├── ConquiTap.sql               # Script de base de datos
├── Program.cs                  # Punto de entrada
├── AppColors.cs                # Paleta de colores y estilos
│
├── Models/
│   └── Models.cs               # Entidades: Usuario, Miembro, Club, etc.
│
├── Helpers/
│   ├── DatabaseHelper.cs       # ADO.NET: consultas y conexión
│   ├── PasswordHelper.cs       # Hash PBKDF2/SHA-256
│   └── SessionManager.cs       # Estado de sesión actual
│
├── Repositories/
│   ├── UsuarioRepository.cs    # CRUD de usuarios
│   ├── MiembroRepository.cs    # CRUD de miembros + catálogos
│   └── ClubRepository.cs       # CRUD de clubes
│
├── Forms/
│   ├── frmLogin.cs             # Pantalla de inicio de sesión
│   ├── frmSetupAdmin.cs        # Asistente de configuración inicial
│   ├── frmMain.cs              # Ventana principal con sidebar
│   ├── frmMiembroDetalle.cs    # Formulario agregar/editar miembro
│   ├── frmClubDetalle.cs       # Formulario agregar/editar club
│   └── frmUsuarioDetalle.cs    # Formulario agregar/editar usuario
│
└── Controls/
    ├── ucDashboard.cs           # Pestaña: Dashboard con estadísticas
    ├── ucMiembros.cs            # Pestaña: Lista de miembros
    ├── ucClubes.cs              # Pestaña: Lista de clubes
    ├── ucPerfil.cs              # Pestaña: Mi perfil
    ├── ucUsuarios.cs            # Pestaña: Gestión de usuarios
    └── ucSistema.cs             # Pestaña: Administración del sistema
```

---

## Roles y permisos

| Función | Miembro | Directivo | Administrador |
|---|:---:|:---:|:---:|
| Ver Dashboard | ✅ | ✅ | ✅ |
| Ver miembros de su club | ✅ | ✅ | ✅ |
| Crear / editar miembros | ❌ | ✅ | ✅ |
| Ver todos los clubes | ✅ | ✅ | ✅ |
| Crear / editar clubes | ❌ | ✅ | ✅ |
| Editar sus propios datos | ✅ | ✅ | ✅ |
| Cambiar su contraseña | ✅ | ✅ | ✅ |
| Gestionar usuarios | ❌ | 👁 Ver | ✅ |
| Crear / editar usuarios | ❌ | ❌ | ✅ |
| Panel de sistema / logs | ❌ | ❌ | ✅ |

---

## Colores institucionales

| Color | Hex | Uso |
|---|---|---|
| **Denim** | `#2f557f` | Color primario, header sidebar, botones |
| **Ming** | `#3e8391` | Color secundario, indicador activo |

---

## Extensibilidad sugerida

- **Reportes**: Agregar exportación a PDF/Excel con Crystal Reports o NPOI
- **Fotos de miembros**: Columna `Foto VARBINARY(MAX)` en la tabla `Miembros`
- **Eventos y actividades**: Nueva tabla `Eventos` con asistencia de miembros
- **Notificaciones**: Sistema de alertas para renovaciones de clases / investiduras
- **Multi-idioma**: Soporte en inglés para cooperación internacional

---

*Desarrollado para ADONE — Iglesia Adventista del Séptimo Día*
