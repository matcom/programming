from typing import List
import random
from manim import *


class BinarySearch(Scene):
    def construct(self):
        rand = random.Random(42)

        array = Array(sorted([rand.randint(1,100) for _ in range(20)]))
        array.vgroup.move_to((0, 0, 0))

        l = 0
        r = len(array.items) -1
        m = (l+r)//2

        x = array.items[rand.randint(0, len(array.items) -1)]
        text = Text(f"Query: {x}", font_size=24).next_to(array.vgroup, 5*UP)

        left = ArrayPointer(array, "L", l, UP)
        right = ArrayPointer(array, "R", r, UP)

        self.play(Create(array.vgroup))
        self.play(Create(text))
        self.wait()
        self.play(Create(left.obj), Create(right.obj))

        self.wait()

        while (l <= r):
            m = (l+r)//2

            middle = ArrayPointer(array, "M", m, dir=DOWN)
            self.play(Create(middle.obj))

            if array.items[m] == x:
                self.play(Indicate(array.squares[m]))
                break
            elif array.items[m] > x:
                r = m - 1
                self.play(right.update(r))
            else:
                l = m + 1
                self.play(left.update(l))

            self.play(FadeOut(middle.obj))


class BubbleSort(Scene):
    def construct(self):
        rand = random.Random(60)

        array = Array([rand.randint(1,100) for _ in range(10)])
        array.vgroup.move_to((0, 0, 0))

        l = 0
        pointer = ArrayPointer(array, "i", l, UP)

        self.play(Create(array.vgroup))
        self.wait()
        self.play(Create(pointer.obj))
        self.wait()

        iter = 1
        iteration = always_redraw(lambda : Text(f"Iteration: {iter}", font_size=24).shift(UP*2))
        self.play(Create(iteration))

        while iter <= len(array.items):
            for i in range(0, len(array.items) - iter):
                if i > 0:
                    self.play(pointer.update(i), run_time=0.5)

                if array.items[i] > array.items[i+1]:
                    self.play(array.swap(i, i+1), run_time=0.5)

            iter = iter+1
            self.play(pointer.update(0), run_time=0.5)


class Array:
    def __init__(self, items, square_size=0.5, font_size=18, buffer=0.1):
        self.items = list(items)
        self.squares: List[Mobject] = []

        self.square_size = square_size
        self.buffer = buffer
        self.displacement = self.square_size + self.buffer

        for i, x in enumerate(items):
            s = Square(side_length=square_size).shift(
                RIGHT * (self.displacement) * i
            )
            t = Text(str(x), font_size=font_size).move_to(s.get_center())
            g = VGroup(s, t)
            self.squares.append(g)

        self.vgroup = VGroup(*self.squares)

    def swap(self, i, j):
        loc_i = self.squares[i].get_center()
        loc_j = self.squares[j].get_center()

        animation = AnimationGroup(
            MoveItemAnimation(
                self.squares[i], end=loc_j, shift=UP * self.displacement
            ),
            MoveItemAnimation(
                self.squares[j], end=loc_i, shift=DOWN * self.displacement if abs(j-i) > 1 else 0
            ),
        )

        self.items[i], self.items[j] = self.items[j], self.items[i]
        self.squares[i], self.squares[j] = self.squares[j], self.squares[i]

        return animation


class ArrayPointer:
    def __init__(self, array: Array, name, index=0, dir=DOWN, font_size=20):
        self.array = array
        self.name = name
        self.index = index
        self.dir = dir

        self.text = Text(self.name, font_size=font_size)
        self.arrow = Arrow(start=self.text.get_top(), end=self.text.get_top() + self.dir)
        self.obj = VGroup(self.text, self.arrow).next_to(array.squares[index], -1 * self.dir)

    def update(self, index):
        self.index = index
        return self.obj.animate.next_to(self.array.squares[index], -1*self.dir)


class MoveItemAnimation(Animation):
    def __init__(self, mobject: Mobject, *args, end, shift, **kwargs):
        super().__init__(mobject, *args, **kwargs)

        self.end = end
        self.direction = shift
        self.zero_step = mobject.get_center()
        self.first_step = mobject.get_center() + shift
        self.second_step = end + shift
        self.third_step = end

    def interpolate_mobject(self, alpha: float) -> None:
        if alpha <= 0.2:
            self.mobject.move_to(self.zero_step + (self.first_step - self.zero_step) * alpha * 5)
        elif alpha <= 0.8:
            self.mobject.move_to(self.first_step + (self.second_step - self.first_step) * (alpha - 0.2) * 1/0.6)
        else:
            self.mobject.move_to(self.second_step + (self.third_step - self.second_step) * (alpha - 0.8) * 5)

    def finish(self) -> None:
        super().finish()
        self.mobject.move_to(self.third_step)