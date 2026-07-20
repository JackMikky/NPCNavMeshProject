using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AssassinNavLinkState : IState
{
    private AssassinNPC npc;
    private Coroutine traversalCoroutine;

    public AssassinNavLinkState(AssassinNPC npc)
    {
        this.npc = npc;
    }

    public void Dispose()
    {
        // no-op
    }

    public void Enter()
    {
        npc.ResetMovementAnimationFlags();
        if (npc.Anim != null) npc.Anim.SetTrigger(AnimationConstants.JumpingNavLinkAnim);

        traversalCoroutine = npc.StartCoroutine(TraverseNavLinkCoroutine());
    }

    public void Exit()
    {
        if (traversalCoroutine != null)
        {
            npc.StopCoroutine(traversalCoroutine);
            traversalCoroutine = null;
        }
    }

    public void Update()
    {
    }

    private IEnumerator TraverseNavLinkCoroutine()
    {
        if (npc.Agent == null) yield break;

        OffMeshLinkData data = npc.Agent.currentOffMeshLinkData;
        Vector3 startPos = npc.transform.position;
        Vector3 endPos = data.endPos;

        // Face the landing point
        Vector3 lookTarget = new Vector3(endPos.x, npc.transform.position.y, endPos.z);
        npc.transform.LookAt(lookTarget);

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;

            // Base horizontal interpolation between start and end
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, normalizedTime);

            float jumpHeight = 1.2f; // Peak jump height
            currentPos.y += Mathf.Sin(normalizedTime * Mathf.PI) * jumpHeight;

            npc.transform.position = currentPos;
            yield return null;
        }

        // Ensure final position exactly matches the NavMesh end point
        npc.transform.position = endPos;

        npc.Agent.CompleteOffMeshLink();

        Debug.Log("[StateMachine] Assassin landed from off-mesh link and resumed pursuit.");

        // after landing, re-evaluate and switch
        npc.TargetDistanceCheckAndTransition();
    }
}