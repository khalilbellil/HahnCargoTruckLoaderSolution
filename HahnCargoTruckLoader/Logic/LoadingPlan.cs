using HahnCargoTruckLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HahnCargoTruckLoader.Helper;

namespace HahnCargoTruckLoader.Logic
{
  public class LoadingPlan
  {
  private readonly Dictionary<int, LoadingInstruction> instructions;
    private readonly Truck truck;
    private readonly List<Crate> crates;
    private bool[,,] cargoSpace;
    private int maxAttempts = 1000;
    private int currentAttempts = 0;

    // Constructor initializes the truck, crates, instructions, and cargo space
    public LoadingPlan(Truck truck, List<Crate> crates) {
        this.truck = truck;
        this.crates = crates;
        this.instructions = new Dictionary<int, LoadingInstruction>();
        this.cargoSpace = new bool[truck.Width, truck.Height, truck.Length];
    }

    // Method to get the loading instructions for all crates
    public Dictionary<int, LoadingInstruction> GetLoadingInstructions() {
        // Start placing crates from the first one
        if (PlaceCrates(0)) {
            return instructions; // Return instructions if successful
        }
        Console.WriteLine($"No solution found after max attempts reached. The crates seems to no fit in the truck !");
        return new Dictionary<int, LoadingInstruction>(); // Return empty if no valid solution
    }

    // Recursive method to place crates using backtracking
    private bool PlaceCrates(int crateIndex)
    {
        if (crateIndex >= crates.Count)
        {
            return true; // All crates have been placed successfully
        }

        var crate = crates[crateIndex];
        var orientations = GetAllOrientations(crate);

        foreach (var orientation in orientations)
        {
            for (int x = 0; x <= truck.Width - orientation.Width; x++)
            {
                for (int y = 0; y <= truck.Height - orientation.Height; y++)
                {
                    for (int z = 0; z <= truck.Length - orientation.Length; z++)
                    {
                        if (CanPlaceCrate(orientation, x, y, z))
                        {
                            PlaceCrate(orientation, x, y, z);
                            instructions[crate.CrateID] = new LoadingInstruction
                            {
                                LoadingStepNumber = instructions.Count + 1,
                                CrateId = crate.CrateID,
                                TopLeftX = x,
                                TopLeftY = y,
                                TurnHorizontal = orientation.Width != crate.Width,
                                TurnVertical = orientation.Height != crate.Height
                            };

                            if (PlaceCrates(crateIndex + 1))
                            {
                                return true; // Successfully placed all remaining crates
                            }

                            // Undo placement if not successful (backtracking)
                            RemoveCrate(orientation, x, y, z);
                            instructions.Remove(crate.CrateID);

                            this.currentAttempts++;

                            // Check if max attempts have been reached
                            if (this.currentAttempts >= this.maxAttempts)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }

        return false; // No valid placement found
    }

    // Generate all possible orientations of a crate
    private List<Crate> GetAllOrientations(Crate crate) {
        var orientations = new List<Crate>();

        // Original orientation
        orientations.Add(new Crate { CrateID = crate.CrateID, Width = crate.Width, Height = crate.Height, Length = crate.Length });
    
        // Rotated orientations
        orientations.Add(new Crate { CrateID = crate.CrateID, Width = crate.Length, Height = crate.Height, Length = crate.Width });
        orientations.Add(new Crate { CrateID = crate.CrateID, Width = crate.Height, Height = crate.Width, Length = crate.Length });
        orientations.Add(new Crate { CrateID = crate.CrateID, Width = crate.Length, Height = crate.Width, Length = crate.Height });
        orientations.Add(new Crate { CrateID = crate.CrateID, Width = crate.Width, Height = crate.Length, Length = crate.Height });
        orientations.Add(new Crate { CrateID = crate.CrateID, Width = crate.Height, Height = crate.Length, Length = crate.Width });

        return orientations;
    }

    // Check if a crate can be placed at a specified position (x, y, z)
    private bool CanPlaceCrate(Crate crate, int x, int y, int z)
    {
        // Check if crate fits within truck dimensions (width, height, length)
        if (x + crate.Width > truck.Width || y + crate.Height > truck.Height || z + crate.Length > truck.Length)
        {
            return false; // Crate exceeds truck dimensions
        }

        // Check if the space is free from any existing crates
        for (int i = 0; i < crate.Width; i++)
        {
            for (int j = 0; j < crate.Height; j++)
            {
                for (int k = 0; k < crate.Length; k++)
                {
                    if (cargoSpace[x + i, y + j, z + k])
                    {
                        return false; // Space occupied
                    }
                }
            }
        }

        return true; // Crate fits and space is available
    }

    // Place a crate in the cargo space and mark it as occupied
    private void PlaceCrate(Crate crate, int x, int y, int z)
    {
        for (int i = 0; i < crate.Width; i++)
        {
            for (int j = 0; j < crate.Height; j++)
            {
                for (int k = 0; k < crate.Length; k++)
                {
                    cargoSpace[x + i, y + j, z + k] = true; // Mark space as occupied
                }
            }
        }
    }

    // Remove a crate from the cargo space (backtracking)
    private void RemoveCrate(Crate crate, int x, int y, int z)
    {
        for (int i = 0; i < crate.Width; i++)
        {
            for (int j = 0; j < crate.Height; j++)
            {
                for (int k = 0; k < crate.Length; k++)
                {
                    cargoSpace[x + i, y + j, z + k] = false; // Unmark space
                }
            }
        }
    }

  }

}