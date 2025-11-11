# 🕹️ Proyecto Final de Animación – Unity 2D Pixel Art  
### *Opción Máxima — Integración de animaciones en entorno interactivo*

---

## 📘 Introducción  

Este proyecto fue desarrollado en **Unity 2022.4.2f1** como parte de la materia **Animación de Videojuegos**.  
El objetivo principal es **integrar animaciones originales dentro de un entorno interactivo**, donde las acciones de los personajes respondan a eventos del juego y entradas del usuario.

El proyecto incluye **dos personajes completamente animados cuadro por cuadro en pixel art**:

| Personaje | Tipo | Control |
|----------|------|---------|
| **Mia** | Protagonista | Jugador |
| **Flor** | Enemigo | IA (FSM + colliders y triggers) |

Ambos aplican principios clásicos de animación adaptados al pixel art y a mecánicas de gameplay 2D.

---

## 🎨 Herramientas utilizadas  

| Área | Software |
|------|----------|
| Pixel Art y Animación | Aseprite |
| Motor de juego | Unity 2022.4.2f1 |
| Scripting | C# + Visual Studio Code |

---

# 🧩 Personajes & Animaciones

---

## 🟣 Mia — Personaje Jugable

🖼️ **Vista previa de animaciones**  
![Mia Preview](docs/gifs/mia.gif)

| Animación | Función | Preview |
|----------|---------|---------------------|
| Idle | Reposo con micro-movimiento | ![Mia Preview](Assets/gifs/Mia-IDLE.gif) |
| Walk/Run | Movimiento horizontal | ![Mia Preview](Assets/gifs/MiaCamina.gif) |
| Jump | Salto con caída | ![Mia Preview](Assets/gifs/MiaSalta.gif) |
| Attack | Ataque cuerpo a cuerpo |![Mia Preview](Assets/gifs/pelea.gif) |

---

## 🌼 Flor — Enemigo (IA)

🖼️ **Vista previa de animaciones**  
![Flor Preview](docs/gifs/flor.gif)

| Animación | Activación | Preview |
|----------|------------|--------------------|
| Idle | Estado base |![Flor Preview](Assets/gifs/Flor-idle.gif)|
| Detect | Entra jugador en rango | ![Flor Preview](Assets/gifs/Flor-detect%20(1).gif) |
| Run | Persigue a Mia | ![Flor Preview](Assets/gifs/flor-corre-gif.gif) |
| Attack | Golpea al jugador | ![Flor Preview](Assets/gifs/flor-ataca.gif) |
| Hit | Recibe daño | ![Flor Preview](Assets/gifs/flor-hit.gif) |
|  Death | Muere con fade-out | ![Flor Preview](Assets/gifs/flor-explodes.gif) |

> 🔹 La UI de vida de *Flor* aparece solo cuando detecta a Mia.

---

# ⚙️ Implementación en Unity

## 🔁 Animators y FSM (Máquinas de Estado)

Cada personaje posee su propio **Animator Controller**, conectado a scripts C# que controlan estados como:

- Movimiento
- Ataque
- Daño
- Muerte
- Transiciones condicionales
- Colliders tipo Trigger para detección y combate

---

## 🎯 Mecánicas principales

| Mecánica | Implementación |
|---------|---------------|
| Ataque de Mia | Hitbox activado por AnimationEvent |
| Daño a Flor | Detecta colisión con hitbox |
| Vida de ambos personajes | UI en corazones actualizada en tiempo real |
| IA de Flor | Detección → persecución → ataque con cooldown |
| Muerte | Animación + fade out + destroy |

