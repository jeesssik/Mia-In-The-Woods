# Mia In The Woods

Proyecto Unity 2D (pixel art) para la materia de Animación de Videojuegos. El objetivo es diseñar y animar personajes aplicando los 12 principios de la animación. Los sprites del personaje principal (Mia) ya están incluidos en `Assets/Sprites`.

## Estado del proyecto
- Etapa inicial (scaffolding).
- Script base: `Assets/Scripts/Mia.cs`.
- Sprites de Mia importados.



## Estructura del proyecto
- `Assets/Sprites/` — Sprites del personaje Mia (pixel art).
- `Assets/Scripts/` — Scripts C# (p. ej. `Mia.cs`).
- `Assets/Scenes/` — Escenas del juego.
- `Packages/` — Dependencias del proyecto.
- `ProjectSettings/` — Configuración de Unity.

## Herramientas de diseño 2D

Se usaron las siguientes herramientas para la creación de arte y animaciones:

- LibreSprite — diseño y animación de Mia (pixel art, sprites y hojas de sprites).
- Pixel Studio — dibujo del background animado en capas.

## Guía de arte (Pixel Art)
- Sprite Mode: Multiple.
- Filter Mode: Point (no filter).
- Compression: None.
- Pixels Per Unit: definir según escala del proyecto.
- Usar Pixel Perfect Camera (paquete 2D Pixel Perfect).

## Principios de animación a aplicar (checklist)
- [ ] Squash & Stretch
- [ ] Anticipation
- [ ] Staging
- [ ] Straight Ahead & Pose to Pose
- [ ] Follow Through & Overlapping Action
- [ ] Slow In & Slow Out
- [ ] Arcs
- [ ] Secondary Action
- [ ] Timing
- [ ] Exaggeration
- [ ] Solid Drawing
- [ ] Appeal

## Personaje: Mia
- Arte: `Assets/Sprites/`.
- Script base: `Assets/Scripts/Mia.cs` (comportamiento inicial y pruebas).
- TODO:
  - [ ] Configurar Animator Controller y estados (idle, walk, jump, hit).
  - [ ] Transiciones con parámetros (Speed, IsGrounded, etc.).
  - [ ] Aplicar principios por clip (anticipation en jump, overlap en cape/hair, etc.).

## Roadmap
- [ ] Crear escena de prueba en `Assets/Scenes/` con cámara Pixel Perfect.
- [ ] Importar y recortar sprites; crear Sprite Library (si corresponde).
- [ ] Montar Animator Controller para Mia.
- [ ] Implementar input básico y movimiento 2D.
- [ ] Integrar clips demostrando cada principio de animación.
- [ ] Capturas/GIFs de referencia en `Docs/` (cuando estén).


