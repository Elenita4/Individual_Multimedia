import imageio_ffmpeg
import os

# Corrección aquí: es os.pathsep (con una sola palabra path)
os.environ["PATH"] += os.pathsep + os.path.dirname(imageio_ffmpeg.get_ffmpeg_exe())

from pydub import AudioSegment

# 1. Cargar el audio original
audio = AudioSegment.from_wav("vaca_lola.wav")

# 2. Cortar el audio para que solo dure los primeros 15 segundos (15000 ms)
audio_corto = audio[:60000]

# 3. Cambiar la frecuencia de muestreo para alterar la voz
robot = audio_corto._spawn(
    audio_corto.raw_data,
    overrides={
        "frame_rate": int(audio_corto.frame_rate * 1.5)
    }
)

# 4. Volver a la frecuencia original
robot = robot.set_frame_rate(audio_corto.frame_rate)

# 5. Exportar el resultado final
robot.export("vaca_lola_robot.wav", format="wav")

print("¡Audio robótico de 15 segundos generado SIN advertencias!")