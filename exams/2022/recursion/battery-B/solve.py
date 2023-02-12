N = 100

for i in range(1,N+1):
    for j in range(i+1,N+1):
        for k in range(j+1,N+1):
            solution = i + j**2 + k**3
            if solution <= N:
                print(f"{i} + {j}^2 + {k}^3 = {solution}")
