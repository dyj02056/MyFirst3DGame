"""Sky Hopper용 효과음을 수학적으로 합성해 WAV 파일로 저장합니다 (표준 라이브러리만 사용).
사용법: python -I make_sfx.py <출력 폴더>
"""
import math
import os
import random
import struct
import sys
import wave

RATE = 44100
random.seed(20261017)


def envelope(t, length, attack=0.005, release=None):
    """짧은 어택 후 끝으로 갈수록 줄어드는 음량"""
    if release is None:
        release = length
    a = min(1.0, t / attack) if attack > 0 else 1.0
    r = max(0.0, 1.0 - t / release)
    return a * r


def tone(freq_start, freq_end, length, wave_type="sine", volume=0.8, vibrato=0.0, vib_rate=0.0, curve=1.0):
    """freq_start에서 freq_end로 미끄러지는 음"""
    samples = []
    phase = 0.0
    n = int(length * RATE)
    for i in range(n):
        t = i / RATE
        k = (t / length) ** curve
        f = freq_start + (freq_end - freq_start) * k
        if vibrato:
            f *= 1.0 + vibrato * math.sin(2 * math.pi * vib_rate * t)
        phase += 2 * math.pi * f / RATE
        if wave_type == "sine":
            s = math.sin(phase)
        elif wave_type == "square":
            s = 0.6 if math.sin(phase) >= 0 else -0.6
        elif wave_type == "triangle":
            s = 2 / math.pi * math.asin(math.sin(phase))
        else:
            s = math.sin(phase)
        samples.append(s * volume * envelope(t, length))
    return samples


def noise(length, volume=0.6, cutoff_start=0.5, cutoff_end=0.05, shape=None):
    """걸러 낸 잡음 (휙, 퍽 같은 소리). cutoff 0~1: 클수록 밝은 소리"""
    samples = []
    low = 0.0
    n = int(length * RATE)
    for i in range(n):
        t = i / RATE
        k = t / length
        c = cutoff_start + (cutoff_end - cutoff_start) * k
        low += c * (random.uniform(-1, 1) - low)
        amp = shape(k) if shape else envelope(t, length)
        samples.append(low * volume * amp * 2.5)
    return samples


def mix(*tracks):
    length = max(len(tr) for tr in tracks)
    out = [0.0] * length
    for tr in tracks:
        for i, s in enumerate(tr):
            out[i] += s
    return out


def concat(*tracks):
    out = []
    for tr in tracks:
        out.extend(tr)
    return out


def silence(length):
    return [0.0] * int(length * RATE)


def note(name):
    """음 이름 → 주파수 (예: C5, E6)"""
    names = {"C": -9, "D": -7, "E": -5, "F": -4, "G": -2, "A": 0, "B": 2}
    semitone = names[name[0]] + (int(name[1:]) - 4) * 12
    return 440.0 * 2 ** (semitone / 12)


def save(path, samples):
    peak = max(1e-6, max(abs(s) for s in samples))
    gain = 0.9 / peak if peak > 0.9 else 1.0
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        frames = b"".join(struct.pack("<h", int(max(-1, min(1, s * gain)) * 32767)) for s in samples)
        w.writeframes(frames)


def bell(freq, length, volume=0.6):
    """맑은 종소리 (기본음 + 배음)"""
    return mix(tone(freq, freq, length, "sine", volume),
               tone(freq * 2, freq * 2, length * 0.6, "sine", volume * 0.35),
               tone(freq * 3, freq * 3, length * 0.3, "sine", volume * 0.15))


SOUNDS = {
    # 점프: 위로 올라가는 짧은 소리
    "sfx_jump": lambda: tone(280, 720, 0.16, "triangle", 0.8, curve=0.6),
    # 더블점프: 더 높고 반짝이는 소리
    "sfx_double_jump": lambda: mix(tone(520, 1180, 0.17, "triangle", 0.7, curve=0.5),
                                   concat(silence(0.05), tone(1560, 1760, 0.1, "sine", 0.25))),
    # 대시: 휙 하는 바람 소리
    "sfx_dash": lambda: noise(0.24, 0.9, 0.08, 0.6, shape=lambda k: math.sin(math.pi * k) ** 1.5),
    # 코인: 띠링
    "sfx_coin": lambda: concat(tone(note("B5"), note("B5"), 0.07, "square", 0.45),
                               tone(note("E6"), note("E6"), 0.28, "square", 0.45)),
    # 별: 반짝이는 상승 아르페지오
    "sfx_star": lambda: concat(*[bell(note(n), 0.16 if i < 3 else 0.6, 0.55)
                                 for i, n in enumerate(["C6", "E6", "G6", "C7"])]),
    # 체크포인트: 두 음 차임
    "sfx_checkpoint": lambda: concat(bell(note("G5"), 0.18, 0.6), bell(note("C6"), 0.7, 0.6)),
    # 밟기: 뽁
    "sfx_stomp": lambda: mix(tone(240, 80, 0.16, "sine", 0.9, curve=0.5), noise(0.08, 0.5, 0.6, 0.1)),
    # 실패: 내려가는 소리
    "sfx_hurt": lambda: tone(620, 140, 0.4, "square", 0.5, vibrato=0.06, vib_rate=18, curve=0.8),
    # 점프대: 보잉
    "sfx_jump_pad": lambda: tone(140, 620, 0.42, "sine", 0.9, vibrato=0.12, vib_rate=14, curve=0.4),
    # 넉백: 쿵 + 내려가는 소리
    "sfx_knockback": lambda: mix(noise(0.12, 0.8, 0.4, 0.05), tone(380, 120, 0.25, "triangle", 0.6)),
    # 떨어지는 발판: 부서지는 소리
    "sfx_crack": lambda: concat(*[mix(noise(0.05, 0.7, 0.7, 0.2), silence(0.06)) for _ in range(4)]),
    # 클리어: 빰빠밤빠~
    "sfx_clear": lambda: concat(
        *[mix(tone(note(n), note(n), 0.13, "square", 0.35), tone(note(n), note(n), 0.13, "triangle", 0.4))
          for n in ["C5", "E5", "G5"]],
        mix(tone(note("C6"), note("C6"), 0.9, "square", 0.3), tone(note("E5"), note("E5"), 0.9, "triangle", 0.3),
            tone(note("G5"), note("G5"), 0.9, "triangle", 0.3))),
    # 버튼 클릭: 톡
    "sfx_click": lambda: tone(1400, 900, 0.045, "sine", 0.7),
}


def main():
    if len(sys.argv) != 2:
        print("usage: python -I make_sfx.py <output_dir>")
        sys.exit(1)
    out_dir = sys.argv[1]
    os.makedirs(out_dir, exist_ok=True)
    for name, make in SOUNDS.items():
        path = os.path.join(out_dir, name + ".wav")
        samples = make()
        save(path, samples)
        print(f"{name}.wav  {len(samples) / RATE:.2f}s")


if __name__ == "__main__":
    main()
