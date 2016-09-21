//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define ENABLE_OVERFLOW_CHECKS

namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class OperatorHandlers_FromImplicitToExplicitExceptions
    {
#if ENABLE_OVERFLOW_CHECKS
        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplicitExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(BinaryOperator) )]
#endif // ENABLE_OVERFLOW_CHECKS
        private static void Handle_BinaryOperator( PhaseExecution.NotificationContext nc )
        {
            BinaryOperator op = (BinaryOperator)nc.CurrentOperator;

            if(op.CheckOverflow)
            {
                var overflowFlag = CreateOverflowCheck(nc, op);
                var opNew = BinaryOperatorWithCarryOut.New(
                    op.DebugInfo,
                    op.Alu,
                    op.Signed,
                    false,
                    op.FirstResult,
                    overflowFlag,
                    op.FirstArgument,
                    op.SecondArgument);

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

#if ENABLE_OVERFLOW_CHECKS
        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplicitExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(UnaryOperator) )]
#endif // ENABLE_OVERFLOW_CHECKS
        private static void Handle_UnaryOperator( PhaseExecution.NotificationContext nc )
        {
            UnaryOperator op = (UnaryOperator)nc.CurrentOperator;

            if(op.CheckOverflow)
            {
                var overflowFlag = CreateOverflowCheck(nc, op);
                var opNew = UnaryOperatorWithCarryOut.New(
                    op.DebugInfo,
                    op.Alu,
                    op.Signed,
                    false,
                    op.FirstResult,
                    overflowFlag,
                    op.FirstArgument);

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplicitExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(SignExtendOperator) )]
        private static void Handle_SignExtendOperator( PhaseExecution.NotificationContext nc )
        {
            SignExtendOperator op  = (SignExtendOperator)nc.CurrentOperator;
            VariableExpression lhs = op.FirstResult;
            Expression         rhs = op.FirstArgument;
            var                di  = op.DebugInfo;

            if(lhs.Type.Size == rhs.Type.Size && lhs.Type.Size == op.SignificantSize)
            {
                var opNew = SingleAssignmentOperator.New( di, lhs, rhs );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
            else if(op.CheckOverflow)
            {
                var opNew = SignExtendOperator.New( di, op.SignificantSize, false, lhs, rhs );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                VerifyNoLossOfPrecision( nc, opNew, lhs, rhs );
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplicitExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(ZeroExtendOperator) )]
        private static void Handle_ZeroExtendOperator( PhaseExecution.NotificationContext nc )
        {
            ZeroExtendOperator op = (ZeroExtendOperator)nc.CurrentOperator;
            VariableExpression lhs = op.FirstResult;
            Expression         rhs = op.FirstArgument;

            if(lhs.Type.Size == rhs.Type.Size && lhs.Type.Size == op.SignificantSize)
            {
                var opNew = SingleAssignmentOperator.New( op.DebugInfo, lhs, rhs );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
            else if(op.CheckOverflow)
            {
                var opNew = ZeroExtendOperator.New( op.DebugInfo, op.SignificantSize, false, lhs, rhs );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                VerifyNoLossOfPrecision( nc, opNew, lhs, rhs );
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplicitExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(TruncateOperator) )]
        private static void Handle_TruncateOperator( PhaseExecution.NotificationContext nc )
        {
            TruncateOperator   op  = (TruncateOperator)nc.CurrentOperator;
            VariableExpression lhs = op.FirstResult;
            Expression         rhs = op.FirstArgument;

            if(lhs.Type.Size == rhs.Type.Size && lhs.Type.Size == op.SignificantSize)
            {
                var opNew = SingleAssignmentOperator.New( op.DebugInfo, lhs, rhs );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
            else if(op.CheckOverflow)
            {
                TruncateOperator opNew = TruncateOperator.New( op.DebugInfo, op.SignificantSize, false, lhs, rhs );
    
                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                VerifyNoLossOfPrecision( nc, opNew, lhs, rhs );
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplicitExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(NullCheckOperator) )]
        private static void Handle_NullCheckOperator( PhaseExecution.NotificationContext nc )
        {
            Operator                 op      = nc.CurrentOperator;
            CompilationConstraints[] ccArray = nc.CurrentCFG.CompilationConstraintsAtOperator( op );
            Expression               addr    = op.FirstArgument;

            if(addr == nc.CurrentCFG.Arguments[0])
            {
                // We know the 'this' pointer is non-null since the caller will check before calling.
                op.Delete();
            }
            else if(addr.Type is ManagedPointerTypeRepresentation ||
                    addr.Type is UnmanagedPointerTypeRepresentation)
            {
                // Managed pointers are guaranteed non-null by construction, while unmanaged pointers
                // can only be used in an unsafe context and therefore should never be checked.
                op.Delete();
            }
            else if(ControlFlowGraphState.HasCompilationConstraint( ccArray, CompilationConstraints.NullChecks_OFF      ) ||
                    ControlFlowGraphState.HasCompilationConstraint( ccArray, CompilationConstraints.NullChecks_OFF_DEEP )  )
            {
                op.Delete();
            }
            else
            {
                Debugging.DebugInfo debugInfo = op.DebugInfo;
                BasicBlock          current   = op.BasicBlock;
                BasicBlock          continueBB;
                BasicBlock          throwBB;

                SplitAndCall( nc, op, true, nc.TypeSystem.WellKnownMethods.ThreadImpl_ThrowNullException, out continueBB, out throwBB );

                //--//
                                    
                current.AddOperator( BinaryConditionalControlOperator.New( debugInfo, addr, throwBB, continueBB ) );
            }

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplicitExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(OutOfBoundCheckOperator) )]
        private static void Handle_OutOfBoundCheckOperator( PhaseExecution.NotificationContext nc )
        {
            Operator op = nc.CurrentOperator;

            CompilationConstraints[] ccArray = nc.CurrentCFG.CompilationConstraintsAtOperator( op );

            if(ControlFlowGraphState.HasCompilationConstraint( ccArray, CompilationConstraints.BoundsChecks_OFF      ) ||
               ControlFlowGraphState.HasCompilationConstraint( ccArray, CompilationConstraints.BoundsChecks_OFF_DEEP )  )
            {
                op.Delete();
            }
            else
            {
                TypeSystemForCodeTransformation ts         = nc.TypeSystem;
                WellKnownTypes                  wkt        = ts.WellKnownTypes;
                WellKnownFields                 wkf        = ts.WellKnownFields;
                WellKnownMethods                wkm        = ts.WellKnownMethods;
                Debugging.DebugInfo             debugInfo  = op.DebugInfo;
                Expression                      exAddress  = op.FirstArgument;
                Expression                      exIndex    = op.SecondArgument;
                BasicBlock                      currentBB  = op.BasicBlock;
                BasicBlock                      continueBB;
                BasicBlock                      throwBB;

                SplitAndCall( nc, op, true, wkm.ThreadImpl_ThrowIndexOutOfRangeException, out continueBB, out throwBB );

                //--//

                ControlFlowGraphStateForCodeTransformation cfg      = nc.CurrentCFG;
                TemporaryVariableExpression                exLength = cfg.AllocateTemporary( wkt.System_UInt32, null );

                currentBB.AddOperator( LoadInstanceFieldOperator        .New( debugInfo, wkf.ArrayImpl_m_numElements, exLength, exAddress, false                                 ) );
                currentBB.AddOperator( CompareConditionalControlOperator.New( debugInfo, CompareAndSetOperator.ActionCondition.LT, false, exIndex, exLength, throwBB, continueBB ) );
            }

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        private static VariableExpression CreateOverflowCheck(PhaseExecution.NotificationContext nc, Operator op)
        {
            TypeRepresentation boolType = nc.TypeSystem.WellKnownTypes.System_Boolean;
            TemporaryVariableExpression overflowFlag = nc.CurrentCFG.AllocateTemporary(boolType, null);

            BasicBlock continueBB;
            BasicBlock throwBB;
            MethodRepresentation throwOverflow = nc.TypeSystem.WellKnownMethods.ThreadImpl_ThrowOverflowException;
            SplitAndCall(nc, op, true, throwOverflow, out continueBB, out throwBB);

            op.AddOperatorAfter(BinaryConditionalControlOperator.New(op.DebugInfo, overflowFlag, continueBB, throwBB));
            nc.MarkAsModified();
            return overflowFlag;
        }

        private static void SplitAndCall(     PhaseExecution.NotificationContext nc              ,
                                              Operator                           op              ,
                                              bool                               fRemoveOperator ,
                                              MethodRepresentation               md              ,
                                          out BasicBlock                         continueBB      ,
                                          out BasicBlock                         throwBB         )
        {
            Debugging.DebugInfo debugInfo = op.DebugInfo;
            BasicBlock          current   = op.BasicBlock;

            BasicBlock[] basicBlocks = nc.CurrentCFG.DataFlow_SpanningTree_BasicBlocks;

            throwBB = null;

            // Try to find an existing throw block for the same exception.
            foreach(BasicBlock block in basicBlocks)
            {
                var callOp = block.FirstOperator as StaticCallOperator;
                if( (callOp != null) &&
                    (callOp.TargetMethod == md) &&
                    ArrayUtility.ArrayEqualsNotNull( block.ProtectedBy, current.ProtectedBy, 0 ))
                {
                    throwBB = block;
                    break;
                }
            }

            continueBB = current.SplitAtOperator( op, fRemoveOperator, false );

            // We didn't find an existing throw block, so create a new one.
            if(throwBB == null)
            {
               Expression[] rhs = nc.TypeSystem.AddTypePointerToArgumentsOfStaticMethod( md );

                throwBB = NormalBasicBlock.CreateWithSameProtection( current );
                throwBB.AddOperator( StaticCallOperator .New( debugInfo, CallOperator.CallKind.Direct, md, rhs ) );
                throwBB.AddOperator( DeadControlOperator.New( debugInfo ) );
            }

            nc.MarkAsModified();
        }

        private static void VerifyNoLossOfPrecision( PhaseExecution.NotificationContext nc  ,
                                                     Operator                           op  ,
                                                     VariableExpression                 lhs ,
                                                     Expression                         rhs )
        {
            var      cfg    = nc.CurrentCFG;
            var      lhsExt = cfg.AllocateTemporary( rhs.Type, null );
            Operator opNew;

            if(lhs.Type.IsSigned)
            {
                opNew = SignExtendOperator.New( op.DebugInfo, lhs.Type.Size, false, lhsExt, lhs );
            }
            else
            {
                opNew = ZeroExtendOperator.New( op.DebugInfo, lhs.Type.Size, false, lhsExt, lhs );
            }

            op.AddOperatorAfter( opNew );

            //--//

            BasicBlock continueBB;
            BasicBlock throwBB;

            SplitAndCall( nc, opNew.GetNextOperator(), false, nc.TypeSystem.WellKnownMethods.ThreadImpl_ThrowOverflowException, out continueBB, out throwBB );

            //--//

            opNew.BasicBlock.AddOperator( CompareConditionalControlOperator.New( op.DebugInfo, CompareAndSetOperator.ActionCondition.NE, false, lhsExt, rhs, continueBB, throwBB ) );

            nc.MarkAsModified();
        }
    }
}
