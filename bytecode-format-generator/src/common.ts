import simpleGit from 'simple-git';

/**
 * Represents an entire Hermes bytecode format definitions file.
 * This is the root structure that gets written to disk.
 */
export interface DefinitionFile {
    /**
     * The version of Hermes bytecode that the definitions are declared for.
     */
    Version: number;
    /**
     * All of the instruction definitions in the file.
     * @see {@link InstructionDefinition}
     */
    Definitions: InstructionDefinition[];
    /**
     * All of the abstract variants in the file.
     * @see {@link AbstractInstructionDefinition}
     */
    AbstractDefinitions: AbstractInstructionDefinition[];
    /**
     * The commit hash of the Hermes git repository that the bytecode was parsed from.
     */
    GitCommitHash: string;
}

/**
 * The definition of a Hermes instruction's format.
 */
export interface InstructionDefinition {
    /**
     * The one-byte opcode the instruction.
     * This is what is encoded inside of a Hermes bytecode file.
     */
    Opcode: number;
    /**
     * The human-readable text representation of the opcode.
     */
    Name: string;
    /**
     * A list of the types of each operand the instruction takes.
     * Empty if the instruction takes no operands.
     */
    OperandTypes: OperandType[];
    /**
     * Whether or not the instruction is a jump instruction; i.e. it can move the instruction pointer.
     */
    IsJump: boolean;
    /**
     * If this instruction is a variant, the `AbstractDefinition` is the index in the abstract definition table of the abstract form.
     * @see {@link AbstractInstructionDefinition}
     */
    AbstractDefinition?: number;
}

/**
 * Represents the type of an operand; i.e. how it is encoded in the binary file.
 */
export enum OperandType {
    Reg8 = 'Reg8',
    Reg32 = 'Reg32',
    UInt8 = 'UInt8',
    UInt16 = 'UInt16',
    UInt32 = 'UInt32',
    Addr8 = 'Addr8',
    Addr32 = 'Addr32',
    Imm32 = 'Imm32',
    Double = 'Double',
    UInt8S = 'UInt8S',
    UInt16S = 'UInt16S',
    UInt32S = 'UInt32S',
}

/**
 * Represents the definition of the abstract form of variant instructions.
 *
 * Variant instructions are ones such as `NewObjectWithBuffer` and `NewObjectWithBufferLong` --
 * they both perform the same action but take different length operands.
 *
 * When hasmer is disassembling a file (and the `--exact` option is omitted),
 * it takes all variant instructions and converts them into an abstracted form (i.e. this structure).
 *
 * As an example, the abstracted form of the variants `NewObjectWithBuffer` and `NewObjectWithBufferLong` is `NewObjectWithBuffer`.
 * To the perspective of the programmer, `NewObjectWithBuffer` has arbitrary length operands.
 *
 * When hasmer then assembles that file, the assembler does optimizations itself to determine which variant should be used,
 * based on the operands that were passed to it.
 *
 * We do this because it would be painstakingly difficult for the programmer to figure out which operand to use.
 */
export interface AbstractInstructionDefinition {
    /**
     * The name of the abstracted instruction.
     */
    Name: string;
    /**
     * The opcodes of the actual variants that are abstracted by this definition.
     */
    VariantOpcodes: number[];
}

/**
 * The instance of the git client.
 */
export const git = simpleGit();

/**
 * Clones the Hermes git repository to the current workiung directory.
 */
export async function cloneHermesRepository() {
    await git.clone('https://github.com/facebook/hermes.git');
}
