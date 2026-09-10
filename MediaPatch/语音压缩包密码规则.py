import base64
import struct
import zipfile
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
        self.mti = self.N + 1

        self.mt[0] = seed & 0xFFFFFFFF

        for i in range(1, self.N):
            self.mt[i] = (
                1812433253
                * (self.mt[i - 1] ^ (self.mt[i - 1] >> 30))
                + i
            ) & 0xFFFFFFFF

        self.mti = self.N

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

    def next_int31(self):
        return self.next_int32() >> 1

    def next_bytes(self, length):
        result = bytearray(length)

        for i in range(0, length, 4):
            value = struct.pack("<I", self.next_int31())

            for j in range(4):
                if i + j >= length:
                    break

                result[i + j] = value[j]

        return bytes(result)


def zip_password(zip_name):
    # 关键：
    # Blue Archive 这里使用小写 ZIP 名称
    zip_name = zip_name.lower()

    if not zip_name.endswith(".zip"):
        zip_name += ".zip"

    seed = xxh32(zip_name.encode("utf-8")).intdigest()

    mt = MersenneTwister(seed)

    # 20 个 Base64 字符对应 15 bytes
    password_bytes = mt.next_bytes(15)

    return base64.b64encode(password_bytes).decode("ascii")


def extract_zip(zip_path, output_path):
    # 使用文件名，而不是完整路径
    zip_name = zip_path.split("/")[-1].split("\\")[-1]

    password = zip_password(zip_name)

    print("ZIP:", zip_name)
    print("Password:", password)

    with zipfile.ZipFile(zip_path, "r") as z:
        z.extractall(
            output_path,
            pwd=password.encode("utf-8")
        )

    print("解压完成")


if __name__ == "__main__":
    extract_zip(
        "JP_Airi.zip",
        "./output"
    )
