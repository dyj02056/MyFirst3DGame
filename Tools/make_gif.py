"""자동 시연(DemoRecorder)으로 녹화한 PNG 프레임들을 README용 GIF 하나로 합칩니다.

사용법: python -I make_gif.py <프레임 폴더> <출력 GIF> [--step 2] [--width 480] [--colors 128]
  --step   몇 장마다 한 장씩 쓸지 (녹화 30fps, step 2 → GIF 15fps)
  --width  GIF 가로 크기 (세로는 비율 유지)
  --colors 색 수 (적을수록 파일이 작아짐, 최대 256)
필요: Pillow (pip install pillow)
"""
import argparse
import glob
import os

from PIL import Image

CAPTURE_FPS = 30


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("frames")
    parser.add_argument("output")
    parser.add_argument("--step", type=int, default=2)
    parser.add_argument("--width", type=int, default=480)
    parser.add_argument("--colors", type=int, default=128)
    args = parser.parse_args()

    paths = sorted(glob.glob(os.path.join(args.frames, "frame_*.png")))[:: args.step]
    if not paths:
        raise SystemExit("프레임이 없습니다: " + args.frames)

    frames = []
    for path in paths:
        image = Image.open(path).convert("RGB")
        height = round(image.height * args.width / image.width)
        frames.append(image.resize((args.width, height), Image.LANCZOS))

    # 모든 프레임이 같은 색표를 쓰게 해서 깜빡임을 줄임 (여러 프레임을 이어 붙인 그림에서 색 뽑기)
    samples = frames[:: max(1, len(frames) // 12)]
    sheet = Image.new("RGB", (args.width, frames[0].height * len(samples)))
    for i, sample in enumerate(samples):
        sheet.paste(sample, (0, i * frames[0].height))
    palette = sheet.quantize(colors=args.colors, method=Image.Quantize.MEDIANCUT)

    indexed = [frame.quantize(palette=palette, dither=Image.Dither.NONE) for frame in frames]
    duration = round(1000 * args.step / CAPTURE_FPS)
    indexed[0].save(args.output, save_all=True, append_images=indexed[1:], duration=duration,
                    loop=0, optimize=True, disposal=1)

    size_mb = os.path.getsize(args.output) / (1024 * 1024)
    print(f"{args.output}: {len(indexed)} frames, {args.width}x{frames[0].height}, "
          f"{len(indexed) * duration / 1000:.1f}s, {size_mb:.2f} MB")


if __name__ == "__main__":
    main()
