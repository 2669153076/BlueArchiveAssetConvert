import base64
import struct
import zipfile
from pathlib import Path

from xxhash import xxh32


class MersenneTwister:
    N = 624
    M = 397
    MATRIX_A = 0x9908B0DF
    UPPER_MASK = 0x80000000
    LOWER_MASK = 0x7FFFFFFF
    AG01 = [0, MATRIX_A]

    def __init__(self, seed):
        self.mt = [0] * self.N
        self.mti = self.N

        self.mt[0] = seed & 0xFFFFFFFF

        for i in range(1, self.N):
            self.mt[i] = (
                1812433253
                * (self.mt[i - 1] ^ (self.mt[i - 1] >> 30))
                + i
            ) & 0xFFFFFFFF

    def next_int32(self):
        if self.mti >= self.N:
            for kk in range(self.N - self.M):
                y = (
                    (self.mt[kk] & self.UPPER_MASK)
                    | (self.mt[kk + 1] & self.LOWER_MASK)
                )

                self.mt[kk] = (
                    self.mt[kk + self.M]
                    ^ (y >> 1)
                    ^ self.AG01[y & 1]
                ) & 0xFFFFFFFF

            for kk in range(self.N - self.M, self.N - 1):
                y = (
                    (self.mt[kk] & self.UPPER_MASK)
                    | (self.mt[kk + 1] & self.LOWER_MASK)
                )

                self.mt[kk] = (
                    self.mt[kk + (self.M - self.N)]
                    ^ (y >> 1)
                    ^ self.AG01[y & 1]
                ) & 0xFFFFFFFF

            y = (
                (self.mt[self.N - 1] & self.UPPER_MASK)
                | (self.mt[0] & self.LOWER_MASK)
            )

            self.mt[self.N - 1] = (
                self.mt[self.M - 1]
                ^ (y >> 1)
                ^ self.AG01[y & 1]
            ) & 0xFFFFFFFF

            self.mti = 0

        y = self.mt[self.mti]
        self.mti += 1

        y ^= y >> 11
        y ^= (y << 7) & 0x9D2C5680
        y ^= (y << 15) & 0xEFC60000
        y ^= y >> 18

        return y & 0xFFFFFFFF

    def next_bytes(self, length):
        result = bytearray()

        while len(result) < length:
            value = struct.pack("<I", self.next_int32() >> 1)
            result.extend(value)

        return bytes(result[:length])


def zip_password(zip_name):
    # Blue Archive 的 ZIP 密码计算使用小写文件名
    zip_name = zip_name.lower()

    if not zip_name.endswith(".zip"):
        zip_name += ".zip"

    seed = xxh32(zip_name.encode("utf-8")).intdigest()

    mt = MersenneTwister(seed)

    # 20 个 Base64 字符 = 15 bytes
    password_bytes = mt.next_bytes(15)

    return base64.b64encode(password_bytes).decode("ascii")


def extract_all_zips():
    current_dir = Path.cwd()

    zip_files = list(current_dir.glob("*.zip"))

    if not zip_files:
        print("当前文件夹没有 ZIP 文件。")
        return

    print(f"找到 {len(zip_files)} 个 ZIP 文件\n")

    for zip_path in zip_files:
        zip_name = zip_path.name
        output_dir = current_dir / zip_path.stem

        print("=" * 60)
        print(f"文件: {zip_name}")

        # 计算密码
        password = zip_password(zip_name)

        print(f"密码: {password}")
        print(f"输出: {output_dir}")

        try:
            output_dir.mkdir(parents=True, exist_ok=True)

            with zipfile.ZipFile(zip_path, "r") as z:
                z.extractall(
                    output_dir,
                    pwd=password.encode("utf-8")
                )

            print("✓ 解压成功")

        except RuntimeError as e:
            print(f"✗ 密码错误: {e}")

        except zipfile.BadZipFile as e:
            print(f"✗ ZIP 文件损坏或格式不支持: {e}")

        except Exception as e:
            print(f"✗ 解压失败: {type(e).__name__}: {e}")

    print("\n全部处理完成。")


if __name__ == "__main__":
    extract_all_zips()
