# 00 - Hello World

## Objectives
1. **Algorithm Design**: Introduce algorithm design through finding the shortest person in a list, including correctness and efficiency.
2. **Terminal Usage**: Learn to create a new .NET solution and project.
3. **First Program**: Write a "Hello World" application.
4. **Project Workflow**: Understand the development process from a blank slate to a working console application, including file locations.
5. **GitHub Introduction**: Create a GitHub profile and upload code to a repository.

## Lecture Structure

### 1. Introduction (5 minutes)
- **Why Learn Programming?**
  - Discuss the relevance of programming in various fields and its importance in problem-solving.

### 2. Algorithm Design Activity (30 minutes)
- **Finding the Shortest Person Algorithm**
  - Introduce the concept of algorithms and their significance in programming.
  - Guide students through designing an algorithm on the blackboard:
    - Define inputs (list of heights).
    - Use a loop invariant to find the shortest height.

#### Example Algorithm Steps:
1. Initialize `shortest` as infinity.
2. Loop through each height in the list:
   - If height < shortest, update shortest.
3. Output shortest height.

- **Correctness**
  - Discuss how to prove that the algorithm correctly identifies the shortest person:
    - Use examples to demonstrate that for any input list, the algorithm will return the correct result.

- **Efficiency**
  - Introduce the concept of efficiency in algorithms, focusing on time complexity.
  - Explain that this algorithm runs in $$O(N)$$ time, where $$N$$ is the number of people in the list.

- **Adversary Technique**
  - Briefly explain the adversary technique for proving lower bounds:
    - Discuss how any algorithm that finds the shortest person must examine each height at least once, thus requiring at least $$N$$ comparisons.
    - Emphasize that this establishes a lower bound on time complexity for this problem.

### 3. Setting Up the Environment (15 minutes)
- **Using the Terminal**
  - Demonstrate how to open the terminal and navigate directories.
  - Command to create a new .NET solution:
    ```bash
    dotnet new sln -n MyFirstSolution
    ```
  - Command to create a new console project:
    ```bash
    dotnet new console -n HelloWorld
    ```
- **File Structure Overview**
  - Explain where generated files are located:
    - Solution file (.sln)
    - Project folder containing Program.cs and other files.

### 4. Writing Your First Program (15 minutes)
- **Hello World Program**
  - Open `Program.cs` and write the following code:
    ```csharp
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
    ```
  - Run the program using:
    ```bash
    dotnet run
    ```

### 5. Understanding the Workflow (10 minutes)
- **From Blank Slate to Application**
  - Discuss the steps taken from creating a project to running it.
  - Highlight important files and their purposes.

### 6. Introduction to GitHub (10 minutes)
- **Creating a GitHub Profile**
  - Walk through signing up for GitHub.
- **Uploading Code**
  - Demonstrate how to initialize a Git repository:
    ```bash
    git init
    git add .
    git commit -m "Initial commit"
    ```
  - Show how to create a repository on GitHub and push local changes:
    ```bash
    git remote add origin <repository-url>
    git push -u origin master
    ```

## Conclusion (5 minutes)
- Recap key points covered in the lecture.
