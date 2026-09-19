# -*- coding: utf-8 -*-
"""
Lab 5 — Router для шардирования сущности `adoptions` (Animal Shelter).

Реализованы две стратегии определения шарда по shard key = user_id:

  A) ShardRouterMod      — shard = hash(shard_key) % N
  B) ConsistentHashRing  — hash ring с virtual nodes

Хеш-функции совпадают с SQL-реализацией из TASK/05_sharding/sql/02_router_functions.sql:

  hash32(key) = ('x' || substr(md5(key::text), 1, 8))::bit(32)::bigint  -> 0 .. 2^32-1
  hash64(key) = знаковое 64-битное значение из md5(key::text)           -> кольцо

MD5 здесь используется только как стабильный детерминированный хеш для
распределения данных (не для криптографии).
"""

import hashlib


def hash32(key):
    """0 .. 2^32-1. Совпадает с ('x'||substr(md5(key::text),1,8))::bit(32)::bigint."""
    return int(hashlib.md5(str(key).encode()).hexdigest()[:8], 16)


def hash64(key):
    """Знаковое 64-битное значение (позиция на кольце).

    Совпадает с ('x'||substr(md5(key::text),1,16))::bit(64)::bigint в PostgreSQL:
    старший бит трактуется как знак, поэтому порядок точек в SQL и Python совпадает.
    """
    h = int(hashlib.md5(str(key).encode()).hexdigest()[:16], 16)
    return h - (1 << 64) if h >= (1 << 63) else h


class ShardRouterMod:
    """Стратегия A: shard = hash(shard_key) % N."""

    def __init__(self, shards):
        self.shards = list(shards)
        if not self.shards:
            raise ValueError("Нужен хотя бы один шард")
        self.n = len(self.shards)

    def route(self, key):
        return self.shards[hash32(key) % self.n]


class ConsistentHashRing:
    """Стратегия B: Consistent Hash Ring с virtual nodes.

    Имя vnode зависит только от индекса шарда, поэтому добавление нового шарда
    НЕ меняет позиции уже существующих точек кольца — в этом суть устойчивости.
    """

    def __init__(self, shards, vnodes=100):
        shards = list(shards)
        if not shards or len(shards) != len(set(shards)):
            raise ValueError("Шарды должны быть непустыми и уникальными")
        if vnodes < 1:
            raise ValueError("vnodes должен быть >= 1")
        self.vnodes = vnodes
        self.ring = []
        for s in shards:
            for i in range(1, vnodes + 1):
                self.ring.append((hash64("shard-%d-vnode-%d" % (s, i)), s))
        self.ring.sort()
        self.points = [p[0] for p in self.ring]

    def route(self, key):
        """hash(key) -> точка на кольце -> ближайший шард по часовой стрелке."""
        k = hash64(key)
        lo, hi = 0, len(self.points)
        while lo < hi:                      # бинарный поиск первой точки >= k
            mid = (lo + hi) // 2
            if self.points[mid] < k:
                lo = mid + 1
            else:
                hi = mid
        if lo == len(self.points):
            lo = 0                          # замыкание кольца
        return self.ring[lo][1]
