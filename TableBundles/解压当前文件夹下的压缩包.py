import zipfile
import base64
import struct
import os


# ============================================================
# XXHash32
# ============================================================

def xxh32(data: bytes, seed=0):
    PRIME1 = 0x9E3779B1
    PRIME2 = 0x85EBCA77
    PRIME3 = 0xC2B2AE3D
    PRIME4 = 0x27D4EB2F
    PRIME5 = 0x165667B1

    def rol(x, r):
        return ((x << r) | (x >> (32 - r))) & 0xFFFFFFFF

    n = len(data)
    i = 0

    if n >= 16:
        v1 = (seed + PRIME1 + PRIME2) & 0xFFFFFFFF
        v2 = (seed + PRIME2) & 0xFFFFFFFF
        v3 = seed & 0xFFFFFFFF
        v4 = (seed - PRIME1) & 0xFFFFFFFF

        while i <= n - 16:
            val = struct.unpack_from("<I", data, i)[0]
            i += 4
            v1 = rol((v1 + val * PRIME2) & 0xFFFFFFFF, 13)
            v1 = (v1 * PRIME1) & 0xFFFFFFFF

            val = struct.unpack_from("<I", data, i)[0]
            i += 4
            v2 = rol((v2 + val * PRIME2) & 0xFFFFFFFF, 13)
            v2 = (v2 * PRIME1) & 0xFFFFFFFF

            val = struct.unpack_from("<I", data, i)[0]
            i += 4
            v3 = rol((v3 + val * PRIME2) & 0xFFFFFFFF, 13)
            v3 = (v3 * PRIME1) & 0xFFFFFFFF

            val = struct.unpack_from("<I", data, i)[0]
            i += 4
            v4 = rol((v4 + val * PRIME2) & 0xFFFFFFFF, 13)
            v4 = (v4 * PRIME1) & 0xFFFFFFFF

        h = (
            rol(v1, 1) +
            rol(v2, 7) +
            rol(v3, 12) +
            rol(v4, 18)
        ) & 0xFFFFFFFF

    else:
        h = (seed + PRIME5) & 0xFFFFFFFF

    h = (h + n) & 0xFFFFFFFF

    while i + 4 <= n:
        k = struct.unpack_from("<I", data, i)[0]
        i += 4

        h = (h + k * PRIME3) & 0xFFFFFFFF
        h = rol(h, 17)
        h = (h * PRIME4) & 0xFFFFFFFF

    while i < n:
        h = (h + data[i] * PRIME5) & 0xFFFFFFFF
        i += 1

        h = rol(h, 11)
        h = (h * PRIME1) & 0xFFFFFFFF

    h ^= h >> 15
    h = (h * PRIME2) & 0xFFFFFFFF

    h ^= h >> 13
    h = (h * PRIME3) & 0xFFFFFFFF

    h ^= h >> 16

    return h & 0xFFFFFFFF


# ============================================================
# MT19937
# ============================================================

class MT:
    N = 624
    M = 397
    MATRIX_A = 0x9908B0DF

    def __init__(self, seed):
        self.mt = [0] * self.N
        self.mti = self.N

        self.mt[0] = seed & 0xFFFFFFFF

        for i in range(1, self.N):
            self.mt[i] = (
                1812433253 *
                (self.mt[i - 1] ^ (self.mt[i - 1] >> 30))
                + i
            ) & 0xFFFFFFFF

    def next_int32(self):
        if self.mti >= self.N:

            for k in range(self.N - self.M):
                y = (
                    (self.mt[k] & 0x80000000) |
                    (self.mt[k + 1] & 0x7FFFFFFF)
                )

                self.mt[k] = (
                    self.mt[k + self.M]
                    ^ (y >> 1)
                    ^ (self.MATRIX_A if (y & 1) else 0)
                ) & 0xFFFFFFFF

            for k in range(self.N - self.M, self.N - 1):
                y = (
                    (self.mt[k] & 0x80000000) |
                    (self.mt[k + 1] & 0x7FFFFFFF)
                )

                self.mt[k] = (
                    self.mt[k + (self.M - self.N)]
                    ^ (y >> 1)
                    ^ (self.MATRIX_A if (y & 1) else 0)
                ) & 0xFFFFFFFF

            y = (
                (self.mt[self.N - 1] & 0x80000000) |
                (self.mt[0] & 0x7FFFFFFF)
            )

            self.mt[self.N - 1] = (
                self.mt[self.M - 1]
                ^ (y >> 1)
                ^ (self.MATRIX_A if (y & 1) else 0)
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
            value = self.next_int32() >> 1
            result.extend(struct.pack("<I", value))

        return bytes(result[:length])


# ============================================================
# 新版密码算法
# ============================================================

def password_mt(name):
    seed = xxh32(name.encode("utf-8"))

    mt = MT(seed)

    password_bytes = mt.next_bytes(15)

    return base64.b64encode(password_bytes).decode("ascii")


# ============================================================
# 旧版密码算法
# ============================================================

def password_old(name):
    return str(xxh32(name.encode("utf-8")))


# ============================================================
# 生成候选密码
# ============================================================

def get_password_candidates(zip_path):

    zip_name = os.path.basename(zip_path)
    stem = os.path.splitext(zip_name)[0]

    candidates = []

    # --------------------------------------------------------
    # ZIP 文件名
    # --------------------------------------------------------

    names = [
        zip_name,
        zip_name.lower(),
        stem,
        stem.lower(),
    ]

    # --------------------------------------------------------
    # ZIP 内部文件名
    # --------------------------------------------------------

    with zipfile.ZipFile(zip_path) as z:

        for info in z.infolist():

            if not info.is_dir():
                names.append(info.filename)
                names.append(info.filename.lower())
                break

    # --------------------------------------------------------
    # 新版算法 + 旧版算法
    # --------------------------------------------------------

    for name in names:

        pw = password_mt(name)

        if pw not in candidates:
            candidates.append(pw)

        pw = password_old(name)

        if pw not in candidates:
            candidates.append(pw)

    return candidates


# ============================================================
# 验证密码
# ============================================================

def test_password(zip_path, password):

    try:

        with zipfile.ZipFile(zip_path) as z:

            for info in z.infolist():

                if info.is_dir():
                    continue

                # 真正读取并解压，避免仅靠 ZipCrypto header
                z.read(info, pwd=password.encode("utf-8"))

                return True

        return False

    except Exception:

        return False


# ============================================================
# 解压
# ============================================================

def extract_zip(zip_path, password):

    output_dir = os.path.splitext(zip_path)[0]

    os.makedirs(output_dir, exist_ok=True)

    with zipfile.ZipFile(zip_path) as z:

        z.extractall(
            output_dir,
            pwd=password.encode("utf-8")
        )

    return output_dir


# ============================================================
# 单个 ZIP
# ============================================================

def process_zip(zip_path):

    print("=" * 70)
    print("ZIP:", os.path.basename(zip_path))

    candidates = get_password_candidates(zip_path)

    for password in candidates:

        print("尝试密码:", password)

        if test_password(zip_path, password):

            print()
            print("✅ 正确密码:", password)

            output_dir = extract_zip(zip_path, password)

            print("📁 输出目录:", os.path.abspath(output_dir))
            print("✅ 解压完成")

            return password

    print("❌ 找不到正确密码")

    return None


# ============================================================
# 批量处理当前目录
# ============================================================

def main():

    zip_files = [
        f for f in os.listdir(".")
        if f.lower().endswith(".zip")
    ]

    print(f"找到 {len(zip_files)} 个 ZIP 文件。")

    for zip_file in zip_files:
        process_zip(zip_file)

    print()
    print("全部处理完成。")


if __name__ == "__main__":
    main()
