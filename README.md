# BlackWater

Guía técnica para abrir, configurar y entender los sistemas actuales de cámara y movimiento del proyecto.

## Contenido

- [Estructura del proyecto](#estructura-del-proyecto)
- [Puesta en marcha](#puesta-en-marcha)
- [Controles](#controles)
- [CameraFollow](#camerafollow)
- [PlayerMovement](#playermovement)
- [Animator y animaciones](#animator-y-animaciones)
- [Entrada del proyecto](#entrada-del-proyecto)
- [Colisiones, capas y apuntado](#colisiones-capas-y-apuntado)
- [Solución de problemas](#solución-de-problemas)

## Estructura del proyecto

Los recursos propios del juego están agrupados en `Assets/_Project`. Los paquetes, plugins y recursos descargados se mantienen separados, cada uno en su ubicación actual.

```text
Assets/
├── _Project/
│   ├── Animation/                 # Controller y clips del personaje
│   ├── Audio/Song BLACKWATER/     # Audio del juego
│   ├── Characters/                # Recursos de personajes
│   ├── Environment/
│   │   ├── Maps/                  # Modelos y materiales del entorno
│   │   └── Terrain/               # Datos y assets de terreno
│   ├── Input/                     # InputSystem_Actions.inputactions
│   ├── Materials/                 # Materiales propios
│   ├── Props/                     # Objetos del mundo
│   └── Scripts/
│       ├── CameraFollow.cs
│       └── PlayerMovement.cs
├── Scenes/                         # Escenas del proyecto
├── Settings/                       # Configuración de renderizado de Unity
├── AllSkyFree/                     # Recurso de cielos
├── EasyRoads3D/                    # Plugin de caminos
├── EasyRoads3D scenes/             # Escenas de muestra del plugin
├── Polytope Studio/                # Entorno y herramientas del paquete
├── StarterAssets/                  # Recursos de plantilla
└── fog/                             # Recurso de niebla
```

Los archivos `.meta` guardan los GUID e información de importación que Unity usa para enlazar los recursos. Deben conservarse junto a sus archivos. Las carpetas de paquetes se dejan con su estructura de origen porque algunas herramientas pueden depender de sus rutas internas.

## Puesta en marcha

1. Abre el proyecto con la versión de Unity indicada en `ProjectSettings/ProjectVersion.txt`.
2. Abre la escena que quieras ejecutar desde `Assets/Scenes`.
3. En la jerarquía, selecciona el objeto del jugador. Debe tener un `CharacterController` y el script `PlayerMovement`.
4. Asigna el Animator del personaje a `Anim` si no está en el mismo objeto o en uno de sus hijos.
5. En `PlayerMovement`, asigna `Cam` a la cámara de juego. Si el campo queda vacío, el script intenta encontrar la cámara etiquetada `MainCamera`.
6. La cámara necesita los componentes `Camera` y `CameraFollow`. Asigna el jugador al campo `Target` de `CameraFollow`.
7. Asegúrate de que el suelo tenga un collider. Para que el cursor encuentre objetos, esos objetos también necesitan colliders y estar en una capa incluida en `Aim Layers`.
8. Pulsa Play. `V` alterna entre tercera persona e isométrica.

`CameraFollow` busca una textura de cursor visible en el modo isométrico. Si no se asigna ninguna en `Isometric Cursor`, genera una mira al iniciar.

## Controles

| Entrada | Acción |
|---|---|
| `W`, `A`, `S`, `D` o flechas | Mover al personaje en relación con la dirección horizontal de la cámara |
| `Left Shift` | Correr |
| `Space` | Saltar |
| Mouse en tercera persona | Orbitar la cámara |
| Mouse en isométrica | Posicionar la mira |
| Botón derecho mantenido en isométrica | Apuntar: el personaje mira la mira mientras se mueve |
| `Q` / `E` en isométrica | Girar la cámara en pasos |
| Rueda del mouse en isométrica | Acercar o alejar la cámara |
| `V` | Alternar entre tercera persona e isométrica |

El vector de movimiento se calcula con WASD y el frente horizontal de la cámara. El cursor no cambia la dirección de desplazamiento. En isométrica, al mantener quieto al personaje, este mira hacia el cursor; mientras se mueve sin apuntar, mira hacia el rumbo de desplazamiento. Al mantener el botón derecho, mira hacia la mira incluso si se mueve lateralmente o hacia atrás.

## CameraFollow

Archivo: `Assets/_Project/Scripts/CameraFollow.cs`

Este componente se coloca en el objeto de la cámara. Sigue a `Target`, administra los dos modos, procesa los controles de cámara, evita que la cámara atraviese obstáculos y cambia el estado del cursor.

### Modos de cámara

**Tercera persona:** el mouse ajusta yaw (giro horizontal) y pitch (inclinación vertical). La cámara calcula una posición detrás del punto de enfoque del personaje. `Distance` controla la separación, y `Height Offset` eleva el enfoque. `Min Pitch` y `Max Pitch` limitan la inclinación.

**Isométrica:** el mouse no rota la cámara. `Isometric Yaw` define el ángulo horizontal; `Isometric Distance` controla la separación horizontal y `Isometric Height` añade altura sobre el enfoque. Q y E suman o restan `Isometric Rotation Step`. La rueda cambia la distancia y la limita entre `Min Isometric Distance` y `Max Isometric Distance`.

Al alternar con `Switch Key`, el cursor queda bloqueado y oculto en tercera persona. En isométrica se libera y muestra la textura asignada o la mira generada.

### Campos del Inspector

| Grupo y campo | Para qué sirve |
|---|---|
| `Target` | Transform que la cámara sigue; normalmente, el jugador. |
| `Distance` | Distancia de la cámara de tercera persona. |
| `Height Offset` | Altura del punto al que ambas cámaras siguen y miran. |
| `Mouse Sensitivity` | Velocidad de órbita en tercera persona. |
| `Min Pitch`, `Max Pitch` | Límites verticales de la órbita. |
| `Isometric Height` | Altura adicional de la cámara isométrica. |
| `Isometric Distance` | Distancia horizontal inicial de la cámara isométrica. |
| `Isometric Yaw` | Orientación horizontal inicial de la vista isométrica. |
| `Isometric Zoom Speed` | Sensibilidad de la rueda en isométrica. |
| `Min/Max Isometric Distance` | Límites del zoom isométrico. |
| `Isometric Rotation Step` | Grados que gira cada pulsación de Q o E. |
| `Rotate Isometric Left/Right Key` | Teclas para los pasos de giro. |
| `Isometric Cursor` | Textura opcional para la mira. Vacía significa usar la mira generada. |
| `Cursor Hotspot` | Punto de la textura que coincide con la posición del mouse. |
| `Collision Layers` | Capas de geometría que pueden bloquear la cámara. |
| `Collision Radius` | Radio de la esfera usada para detectar obstáculos. |
| `Collision Padding` | Margen que se deja entre la cámara y el obstáculo. |
| `Position Smooth Time` | Suavizado al seguir el punto deseado cuando el camino está libre. Cero desactiva ese suavizado. |
| `Rotation Sharpness` | Rapidez con la que la cámara orienta su rotación hacia la deseada. |
| `Switch Key` | Tecla que alterna entre los dos modos. |

### Colisión de cámara

Para cada actualización, el script barre una esfera desde el enfoque hasta la posición deseada. Si encuentra un collider en `Collision Layers`, acerca la cámara hasta antes del obstáculo y resta `Collision Padding`. La jerarquía del objetivo se ignora para que el collider del personaje no bloquee su propia cámara; los triggers tampoco bloquean.

La cámara se acerca inmediatamente cuando encuentra una obstrucción. Cuando el trayecto queda libre, vuelve hacia la posición calculada con el suavizado de posición. Si atraviesa una pared, revisa que tenga collider activo y que su capa esté incluida en `Collision Layers`.

### Mira isométrica

La mira predeterminada es una textura pequeña generada en memoria al iniciar: tiene contorno oscuro, brazos blancos y un centro ámbar. Para usar un cursor propio, importa una textura, asígnala a `Isometric Cursor` y ajusta `Cursor Hotspot`. En tercera persona, el sistema restaura el cursor del sistema y lo bloquea para permitir la órbita.

## PlayerMovement

Archivo: `Assets/_Project/Scripts/PlayerMovement.cs`

Este componente debe estar en el jugador. Lee el teclado, calcula el movimiento relativo a la cámara, rota el personaje, usa `CharacterController` para moverlo, administra salto y gravedad y comunica parámetros al Animator.

### Referencias

| Campo | Para qué sirve |
|---|---|
| `Controller` | CharacterController utilizado para comprobar el suelo y mover al jugador. Si queda vacío, se busca en el mismo objeto. |
| `Cam` | Transform de la cámara. Si falta, se intenta encontrar `Camera.main`. |
| `Anim` | Animator del personaje; si falta, se busca en los hijos. |
| `Camera Follow` | Permite saber si está activa la vista isométrica. Si queda vacío, se busca `CameraFollow` en la cámara encontrada. |

Las búsquedas automáticas ayudan con configuraciones simples. Para evitar referencias ambiguas cuando la escena tenga varias cámaras o animadores, asígnalas explícitamente en el Inspector.

### Movimiento y giro

El script lee los ejes clásicos `Horizontal` y `Vertical`, limita la entrada diagonal para que no sea más rápida y proyecta el frente de la cámara sobre el plano horizontal. `A/D` usan el eje lateral derivado de ese frente. Así el movimiento sigue la orientación de la cámara, no la dirección de la mira.

`Walk Speed` y `Run Speed` determinan la velocidad deseada. Mantener `Left Shift` selecciona la velocidad de carrera. `Acceleration` controla la rapidez con que el jugador gana velocidad y `Deceleration` la rapidez con que la pierde al soltar las teclas. En el aire, `Air Control` multiplica ese control por un valor de cero a uno. `Turn Speed` limita cuántos grados puede rotar el personaje por segundo.

En isométrica, el personaje normalmente mira hacia donde se desplaza. Si está quieto, mira hacia el cursor. Si se mantiene el botón definido por `Aim Mouse Button`, mira hacia el cursor incluso durante el movimiento. En tercera persona, el cursor de apuntado isométrico no se usa para orientar al jugador.

### Apuntado

El script proyecta un rayo desde la cámara por la posición de pantalla del mouse. Primero busca el collider válido más cercano en `Aim Layers` y omite los colliders del jugador. Si no encuentra una superficie, cruza ese rayo con un plano horizontal a la altura del personaje. Ese plano permite que la mira siga produciendo un punto de mundo incluso si el terreno no tiene collider, aunque no puede seguir las alturas y formas reales del terreno.

| Campo o propiedad | Para qué sirve |
|---|---|
| `Aim Mouse Button` | Botón mantenido para apuntar; el valor predeterminado `1` es el botón derecho. |
| `Aim Layers` | Capas cuyos colliders puede alcanzar el rayo. |
| `Aim Ray Distance` | Distancia máxima de búsqueda de colliders. |
| `IsAiming` | Propiedad de solo lectura que indica que el botón está mantenido y se encontró un punto de mira en isométrica. |
| `AimPoint` | Último punto del mundo encontrado por el rayo o por el plano de respaldo; vale cero cuando no hay punto. |

`Aim Layers` no controla el plano de respaldo. Si se apunta a terreno con alturas distintas, asegúrate de que el terreno tenga collider y su capa esté incluida.

### Salto y gravedad

El estado de suelo se obtiene con `CharacterController.isGrounded`. Al aterrizar, la velocidad vertical descendente se fija a un valor pequeño negativo para mantener el contacto con el piso. Al pulsar `Space` estando en el suelo, `Jump Height` y `Gravity` determinan la velocidad inicial del salto; la gravedad se aplica cada frame.

`Gravity` debe ser negativa para que la gravedad y la fórmula de salto funcionen correctamente. `Jump Height` representa la altura aproximada deseada del salto, en unidades de Unity.

## Animator y animaciones

El script envía dos parámetros al Animator:

- `IsGrounded` (Bool): se actualiza desde el estado del CharacterController.
- `Speed` (Float): magnitud de la velocidad horizontal actual, después de aceleración y desaceleración.

Los nombres y tipos deben coincidir exactamente con los parámetros del Animator. El movimiento diagonal queda limitado y `Speed` refleja la velocidad real, por lo que puede tardar en alcanzar la velocidad configurada.

El conjunto de clips actual incluye reposo, caminar, correr y salto. No se detectaron clips de desplazamiento lateral o hacia atrás. Por eso, al apuntar y moverse de lado o hacia atrás, el personaje puede desplazarse mientras reproduce una animación frontal. Para animación direccional se necesita locomoción adicional y un Blend Tree 2D configurado con las direcciones correspondientes.

## Entrada del proyecto

Los scripts usan el sistema clásico de entrada de Unity: `Input.GetAxisRaw`, `Input.GetAxis`, `Input.GetKey`, `Input.GetMouseButton` e `Input.GetButtonDown`. Por tanto, necesitan los ejes `Horizontal`, `Vertical`, `Mouse X`, `Mouse Y`, `Mouse ScrollWheel` y el botón `Jump` configurados en `Edit > Project Settings > Input Manager`.

El proyecto también incluye el paquete Input System y el archivo `Assets/_Project/Input/InputSystem_Actions.inputactions`, pero estos dos scripts no leen ese archivo: actualmente usan el Input Manager clásico. La configuración activa del proyecto permite ambos sistemas (`Both`). Para cambiar los controles mediante Input Actions habría que adaptar los scripts además de configurar las acciones.

## Colisiones, capas y apuntado

- El jugador necesita `CharacterController`; el componente define la cápsula que se mueve y comprueba el contacto con el suelo.
- Paredes y objetos que deban bloquear la cámara necesitan colliders y capas incluidas en `Collision Layers`.
- Suelo y objetos que deban ser alcanzables por la mira necesitan colliders y capas incluidas en `Aim Layers`.
- Los colliders del propio jugador se excluyen tanto de la colisión de cámara como del rayo de apuntado.
- Un objeto puede bloquear la cámara sin ser apuntable si está solo en `Collision Layers`; también puede ser apuntable sin bloquear la cámara si está solo en `Aim Layers`.
- Los triggers se ignoran en ambas búsquedas físicas.

## Solución de problemas

| Síntoma | Comprobación |
|---|---|
| El personaje no se mueve | Confirma `CharacterController`, revisa los ejes `Horizontal` y `Vertical` en Input Manager y comprueba la consola por errores de compilación. |
| El personaje se mueve, pero no anima | Asigna el Animator correcto y confirma que tenga los parámetros Bool `IsGrounded` y Float `Speed`. |
| No salta | Comprueba el botón `Jump`, que el CharacterController esté tocando el suelo y que `Gravity` tenga valor negativo. |
| El mouse no orbita en tercera persona | Comprueba que la cámara tenga `CameraFollow`, que el modo sea tercera persona y que el cursor esté bloqueado durante Play. |
| El cursor no encuentra el terreno | Añade un collider al terreno y asegúrate de que su capa esté incluida en `Aim Layers`. Sin collider se usa el plano de respaldo. |
| La cámara atraviesa una pared | Añade un collider a la pared y su capa a `Collision Layers`; ajusta `Collision Radius` si hace falta. |
| El personaje mira hacia una dirección rara | Comprueba la orientación frontal del modelo y que su eje Z local apunte al frente esperado. |
| La cámara no sigue al personaje | Asigna `Target` en `CameraFollow` y comprueba que sea el Transform del jugador. |

Los valores de velocidad, giro, gravedad, cámara y capas se pueden ajustar desde el Inspector sin modificar el código. Cambia una variable cada vez durante Play para identificar qué parámetro produce el resultado deseado.

## Git Bash: descargar y subir cambios

Este flujo mantiene el trabajo en una rama propia. La rama se publica en GitHub y otra persona revisa el Pull Request y el estado de integración antes de unirlo a la rama principal.

> En los ejemplos se usa `main` como rama principal. Si el repositorio usa `master` u otro nombre, sustituye `main` por el nombre real. Cambia `feature/mi-cambio` por un nombre corto que describa tu tarea.

### Abrir el repositorio en Git Bash

```bash
cd "/d/Proyectos Unity/BlackWater"
git status
git branch --show-current
```

Antes de cambiar de rama, `git status` debe mostrar que entiendes todos los cambios locales. Si hay archivos modificados que todavía necesitas, guárdalos en un commit o consúltalo antes de continuar; no los descartes para poder cambiar de rama.

### Actualizar la rama principal y crear tu rama de trabajo

Si todavía no tienes cambios locales que quieras conservar:

```bash
git switch main
git pull --ff-only origin main
git switch -c feature/mi-cambio
```

Esto descarga la versión más reciente de `main` y crea una rama nueva a partir de ella. Haz tus cambios en esa rama. Si ya existe una rama remota para tu tarea, actualízala en lugar de crear otra:

```bash
git fetch origin
git switch feature/mi-cambio
git pull --ff-only origin feature/mi-cambio
```

### Revisar, guardar y subir tus cambios

Cuando termines una parte coherente del trabajo:

```bash
git status
git diff --check
git diff
```

Revisa el contenido. Para preparar solo los scripts y la documentación de esta tarea, por ejemplo:

```bash
git add Assets/_Project/Scripts/CameraFollow.cs
git add Assets/_Project/Scripts/PlayerMovement.cs
git add README.md
git diff --cached --stat
git status
```

Si también cambiaste escenas, prefabs u otros recursos de Unity, añade únicamente las rutas que revisaste y que quieres compartir. Después crea el commit y publica tu rama:

```bash
git commit -m "Describe el cambio"
git push -u origin feature/mi-cambio
```

La primera vez, `-u` vincula la rama local con la remota; después puedes usar `git push` y `git pull` desde esa rama.

### Crear el Pull Request en GitHub

1. Abre el repositorio en GitHub. Normalmente aparecerá una opción para crear un Pull Request con la rama que acabas de subir.
2. Selecciona tu rama como origen y `main` como destino.
3. Explica qué cambió y qué verificaste en Unity. Revisa la lista de archivos y el diff antes de pedir revisión.
4. Solicita a otra persona que revise los cambios y el estado de conflictos del Pull Request.
5. Integra la rama solo después de la revisión y de que GitHub indique que se puede combinar. Si GitHub informa conflictos, resuélvelos y vuelve a subir la rama para que la revisen de nuevo.

Subir una rama no integra automáticamente los cambios en `main`. El Pull Request es el espacio para revisar qué archivos van a entrar y si la rama se puede combinar.

### Incorporar cambios nuevos de `main` a tu rama

Si `main` avanzó mientras trabajabas, trae esos cambios a tu rama con `merge`:

```bash
git fetch origin
git switch feature/mi-cambio
git merge origin/main
```

Si el merge termina sin conflictos, comprueba el resultado y publica la rama:

```bash
git status
git push
```

Si aparece un conflicto, Git detiene el merge y muestra qué archivos necesitan atención. No continúes al siguiente paso hasta resolverlos.

### Resolver conflictos

1. Ejecuta `git status` y anota los archivos marcados como `both modified`, `unmerged` o similares.
2. Abre cada archivo en conflicto. En archivos de texto pueden aparecer marcadores como `<<<<<<<`, `=======` y `>>>>>>>`. Conserva el contenido correcto de ambos lados cuando corresponda y elimina los marcadores al terminar.
3. En un merge, `ours` significa la rama en la que estás trabajando y `theirs` significa la rama que estás incorporando. No elijas todo un lado automáticamente: comprueba qué cambios contiene cada uno.
4. En archivos Unity `.unity`, `.prefab` o `.asset`, los conflictos pueden corresponder a objetos o propiedades distintos. Si UnityYAMLMerge (Smart Merge) está configurado para el repositorio, úsalo; después abre el proyecto y comprueba visualmente los recursos resueltos. Si no está configurado, compara con cuidado las versiones y resuelve el archivo con una herramienta de merge adecuada. No borres los marcadores dejando un YAML incompleto.
5. En archivos `.meta`, conserva el GUID del asset que debe seguir siendo el mismo recurso. Regenerar el `.meta` puede romper referencias.
6. Guarda cada archivo resuelto y márcalo como resuelto:

```bash
git add ruta/al/archivo-resuelto
```

7. Cuando `git status` ya no muestre archivos sin resolver, confirma la resolución y publica la rama:

```bash
git status
git diff --check
git commit -m "Resuelve conflictos con main"
git push
```

8. Revisa otra vez el Pull Request de GitHub y avisa a la persona revisora para que compruebe el nuevo estado.

Si necesitas cancelar el merge en curso y volver al estado anterior al intento, usa:

```bash
git merge --abort
```

Esto cancela el merge actual; antes de usarlo, confirma con `git status` que realmente estás dentro de ese merge y que tus cambios previos están guardados. No uses `git reset --hard` para resolver conflictos: puede borrar trabajo local.

### Archivos de Unity y revisión antes de subir

- Mantén cada `.meta` junto a su asset. Los GUID enlazan escenas, prefabs, materiales, scripts y otros recursos.
- Para mover o renombrar assets, usa la ventana Project de Unity cuando sea posible; Unity actualizará el movimiento del `.meta` con el asset.
- Revisa escenas y prefabs con cuidado: un conflicto de texto puede compilar como YAML pero cambiar objetos, componentes o referencias.
- No añadas `Library`, `Temp`, `Logs`, `Obj`, `Builds` ni archivos temporales generados por el Editor. El `.gitignore` del proyecto ya excluye las carpetas generadas habituales; revisa `git status` antes de hacer commit.
- No incluyas cambios ajenos a tu tarea. Inspecciona `git diff --cached` antes de crear el commit.
- Después de un merge de escenas o prefabs, abre la escena afectada en Unity y verifica los objetos y referencias importantes antes de marcar el Pull Request como listo para revisión.


